# User Flows

The 5 core flows for GamePlanPro MVP, in priority order. These define required screens, APIs, and state transitions.

## Flow 1: Create a Practice Plan

1. Coach clicks "Create Practice Plan" from the dashboard
2. System auto-generates a suggested plan name (editable by coach)
3. Coach sets: Location (dropdown), Intended Duration (dropdown), Description (optional text field)
4. System pre-populates default Sections: Warm-up, Practice, Cool-Down
5. Coach can add sections (custom name), remove sections, rename sections, and reorder via drag-and-drop
6. Plan auto-saves throughout; coach does not need to manually save
7. System tracks total plan duration in real time:
   - Sequential drills are summed
   - Station groups (parallel drills) contribute only the duration of their longest drill
   - A non-blocking warning displays if total tracked time exceeds the plan's intended duration
8. Plan appears on the dashboard in a "My Practice Plans" section with created date, plan name, total duration, and description
9. Coach can search and filter plans by plan name or created date
10. Coach can click download icon on a plan from dashboard to download as PDF/printable export
    - PDF includes: plan name, location, total duration, description, expanded sections with individual drills and grouped drills

## Flow 2: Add a Drill to a Section (from Library)

1. Coach clicks "Add from Library" on a Section
2. Library panel opens showing both System Drills and My Drills
   - Filterable by Category and Drill Type
   - Searchable by name or Drill Type
3. Coach selects a drill; it is added to the Section as a Plan Drill instance
4. Coach can edit the Plan Drill's fields (duration, coach assignment, player count, instructions, demo link) without affecting the source Library Entry
5. Coach can click "Remove" to delete the drill

## Flow 3: Create a New Drill Inline

1. Coach clicks "Create New" on a Section
2. A blank Plan Drill form expands inline with fields:
   - Drill Name, Duration, Drill Type, Instructions, Demonstration Link (YouTube URL), Coach Assignment (free text), Player Count (integer)
3. Coach fills in details; drill is saved to the Section
4. Coach can optionally click "Save as Template" to save to their personal My Drills library
5. Coach can click "Remove" to delete the drill

## Flow 3b: Group Drills into a Station (Parallel Execution)

1. Coach clicks "Group as Station" button at the top of the section
2. A "Run Simultaneously" button appears with instructions to select 2+ drills
3. Coach selects two or more Plan Drills within a Section using checkboxes or multi-select
4. Coach clicks "Run Simultaneously"; selected drills are visually grouped with:
   - A shared container or bracket
   - A "Runs simultaneously" label
5. Duration tracking updates immediately: Station contributes the longest drill's duration, not the sum
6. Coach can ungroup a Station at any time — drills return to individual sequential entries
7. Drills within a Station can still be individually edited (name, duration, coach assignment, player count, demo link)

## Flow 4: Browse & Manage Skills & Drills Library

1. Coach navigates to the Skills & Drills page
2. Page shows two tabs: My Drills (personal) and Drill Library (system, 50 curated drills)
3. Search bar filters by drill name or drill type; Filter narrows by category or drill type
4. Coach can create a new drill directly from this page (opens blank drill form)
5. Coach fills in details and clicks "Create Drill" — saved to personal My Drills library
6. Coach can edit or delete drills in My Drills; System Drills are read-only
7. Coach can open any System Drill and click "Save as Template" to create an editable copy in My Drills (System Drill remains unchanged)
8. Each drill card shows: name, drill type badge, description, duration, and hyperlinked video indicator if demo link exists

## Flow 5: View & Edit an Existing Practice Plan

1. Dashboard shows a list of the coach's Practice Plans with name, created date, total duration, description, and download option
2. Coach clicks a plan to open it
3. Full plan is displayed with all sections and drills expanded
4. Coach can edit any field, reorder sections/drills, add/remove drills, or delete the plan
5. Auto-save keeps changes without a manual save step
