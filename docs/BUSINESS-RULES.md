# Business Rules

Constraints enforced in code, not just documentation.

## Practice Plan Rules

- A Practice Plan **must have a name**. Auto-suggest on creation, but coach must confirm or enter one before the plan is visible on the dashboard.
- A Practice Plan belongs to exactly one User and is **private** to that User in MVP (no sharing).
- Practice Plans are **reusable templates** — they do NOT have a scheduled practice date. The date displayed is `created_at` or `last_modified_at`.
- **Duration tracking is required.** Total plan time = sum of all sequential Plan Drill durations + the longest drill duration per Station group. A non-blocking warning displays when tracked time exceeds intended duration. The coach is **never prevented from saving**.
- **Auto-save is required** — no manual "Save" button.

## Section Rules

- Default sections pre-populated on plan creation: **Warm-up, Practice, Cool-Down**.
- Coach can add (custom name), delete, rename, and reorder sections via drag-and-drop.
- Deleting a section also deletes all Plan Drills within it (**with a confirmation prompt**).
- A plan can have zero sections (though the UI should guide coaches toward adding at least one).
- As Plan Drills are added, a tag is added/updated next to the section name showing **total number of drills** in that section.

## Station Rules

- A Station is formed when a coach selects 2+ Plan Drills within a Section and clicks "Run Simultaneously."
- Stations are **not a separate database entity**. Represented by a shared `station_group` UUID on each grouped Plan Drill record.
- A Station's duration contribution = the **longest drill** in the group (not the sum).
- Drills in a Station display with a shared visual container and a "Runs simultaneously" label.
- A Station can be ungrouped at any time. Drills return to sequential entries; `station_group` is cleared.
- A Plan Drill can only belong to **one Station** at a time.

## Drill / Plan Drill Rules

- Duration is in **whole minutes** (integer), minimum 1.
- Coach Assignment is a **free-text string** — NOT a relational FK in MVP. No validation required.
- Player Count is a **positive integer** — optional field. Indicates how many players are needed.
- Demonstration Link accepts a URL string. No strict validation beyond **basic URL format check**.
- **"Save as Template"** copies the Plan Drill's data into a new My Drill entry. Editing the Plan Drill after saving does NOT update the saved template, and vice versa (independent copies).

## Drill Library Rules

- System Drills (Drill Library tab) are **read-only** for all users — cannot be edited or deleted.
- My Drills are owned by a User and can be edited or deleted by **that user only**.
- A User can add a System Drill to their plan **without it appearing in My Drills**. My Drills only populates when the coach explicitly "Saves as Template."
