# UX Specifications

Specific interaction behaviors required for MVP. These are defined requirements, not design suggestions.

## Drill Insertion Behavior

When a coach adds a new drill to a section (via "Add from Library" or "Create New"):

1. New drill is inserted at the **bottom** of the section — preserves sequential order logic (warm-up first, cooldown last)
2. Page **auto-scrolls** to bring the newly added drill card into view
3. The new drill card displays a **brief highlight animation** on arrival — a colored border or background flash lasting 1-2 seconds — to confirm the drill was successfully added
4. This visual confirmation is **required**. Scroll position alone is not sufficient feedback.

**Rationale:** Coaches build plans in sequence. Inserting at the bottom mirrors how they think (drill order = practice order). The highlight animation is the primary signal that the action succeeded.

## Auto-Save

- All plan edits auto-save. No manual "Save" button anywhere in the plan builder.
- Debounce strategy: save after user stops editing (not on every keystroke).
- Visual indicator showing save status (e.g., "Saved" / "Saving...") is recommended.

## Duration Tracking Display

- Total plan duration shown in real time as drills are added/edited/removed.
- Formula: sum of sequential drill durations + longest drill per Station group.
- Non-blocking warning when total exceeds intended duration — coach is never prevented from continuing.

## Section Management

- Sections reorderable via drag-and-drop.
- Drill count tag displayed next to each section name.
- Deleting a section shows a confirmation prompt before removing section and all its drills.

## Plan Dashboard

- "My Practice Plans" list shows: plan name, created date, total duration, description.
- Search and filter by plan name or created date.
- Download icon on each plan for PDF export.

## Drill Library Panel

- Opens from within a Section when coach clicks "Add from Library."
- Shows both System Drills and My Drills.
- Filterable by Category and Drill Type, searchable by name or Drill Type.
- Each drill card: name, drill type badge, description, duration, video indicator if demo link exists.

## Skills & Drills Page

- Two tabs: My Drills, Drill Library.
- System Drills are read-only with a "Save as Template" option.
- My Drills are fully editable and deletable.

## User Mental Model

How "Volunteer Victor" (target coach) thinks about practice planning — UX decisions should align with this:

- Coaches think in **skill categories**: "What are we working on today?" — Hitting? Fielding? Baserunning?
- They think in **time blocks**: "I have 90 minutes. First 15 is warm-up, then 30 on hitting stations..."
- They want to **feel organized and professional** in front of other parents. The plan is their "script." A clean, printable layout matters.
- They **don't distinguish template vs. scheduled event**. A plan IS the practice. Calendar is a future abstraction.
- **Video is a trusted reference.** Coaches rely on YouTube to verify they're teaching drills correctly.
- They will **reuse plans** across multiple practices. The template model is a feature, not a limitation.
- They are **busy**. Auto-save, sensible defaults, and pre-populated sections reduce friction. Every extra click is a reason to abandon.
