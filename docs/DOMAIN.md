# Domain Model

Full entity definitions, relationships, and naming conventions for GamePlanPro MVP.

## Core Entities

### User (Coach)
The authenticated user. Has a profile that stores lightweight team info (`team_name` + `age_group`). Owns Practice Plans and personal drills.

### Practice Plan
A reusable template for a practice session. Created by a User. Has many Sections. Has a name (manually entered or auto-suggested), location, intended duration, and a description/notes field. Date shown = `created_at` / `last_modified_at`. NOT a scheduled calendar event.

### Section
A named block within a Practice Plan (e.g., Warm-up, Practice, Cool-down). Has a name and display order. Coach can add, remove, rename, and reorder sections. Pre-populated defaults on plan creation: Warm-up, Practice, Cool-Down.

### Plan Drill
A drill instance within a Section. NOT the same as a Drill Library Entry. Has: drill name, duration (minutes), instructions, demonstration link (YouTube URL), coach assignment (free-text string), player count (integer), `station_group` (optional UUID linking simultaneous drills). May reference a Drill Library Entry OR be created from scratch.

### Station
A group of 2+ Plan Drills within a Section that run simultaneously (parallel). **Not a separate DB entity** — represented by a shared `station_group` UUID on Plan Drill records. Duration contribution = longest drill in the group, not the sum.

### Drill Library Entry
A reusable drill record. Can be:
- **System Drill** — curated, read-only, available to all coaches (50 at launch). `source = 'system'`
- **My Drill** — user-created or saved from a Plan Drill, editable/deletable by the owning User. `source = 'user'`

Has: name, category, description, duration, instructions, demonstration link.

### Drill Type
A classification tag applied to drills. Examples: Warm-up, Hitting, Pitching, Base Running, Conditioning. Used for searching & filtering in the Drill Library.

### Team (NOT a separate entity in MVP)
Stored as fields on the User profile: `team_name` and `age_group`. No Team management UI in v1 — plumbing only for future Team Manager product.

## Entity Relationships

```
User (Coach)
 ├── team_name, age_group (profile fields, no separate Team entity)
 ├── has many → Practice Plans (private to user in MVP)
 │    └── has many → Sections (ordered)
 │         └── has many → Plan Drills (ordered)
 │              ├── optionally references → Drill Library Entry
 │              └── station_group UUID → groups into Stations (parallel)
 └── has many → My Drills (personal library, source='user')

Drill Library (shared)
 └── System Drills (source='system', read-only, 50 at launch)

Drill Library Entry
 └── belongs to → Drill Type
```

## Naming Conventions

Use these exact names in code, database columns, API routes, and comments.

| Concept | DB (snake_case) | C# (PascalCase) | TypeScript (camelCase) | API route |
|---|---|---|---|---|
| Practice Plan | `practice_plan` | `PracticePlan` | `practicePlan` | `/api/practice-plans` |
| Section | `section` | `Section` | `section` | nested under plan |
| Plan Drill | `plan_drill` | `PlanDrill` | `planDrill` | nested under section |
| Drill Library Entry | `drill` | `Drill` | `drill` | `/api/drills` |
| Drill Type | `drill_type` | `DrillType` | `drillType` | — |
| Station Group | `station_group` | `StationGroup` | `stationGroup` | — |
| Coach Assignment | `coach_assignment` | `CoachAssignment` | `coachAssignment` | — |
| Player Count | `player_count` | `PlayerCount` | `playerCount` | — |
| Demo Link | `demo_link` | `DemoLink` | `demoLink` | — |
| Team Name | `team_name` | `TeamName` | `teamName` | — |
| Age Group | `age_group` | `AgeGroup` | `ageGroup` | — |

## Glossary

| Term | Definition |
|---|---|
| **Practice Plan** | A reusable template for a practice session. NOT a scheduled event. |
| **Section** | A named block within a Practice Plan (e.g., Warm-up). |
| **Plan Drill** | A drill instance within a Section. Independent copy — edits don't affect the source library entry. |
| **Drill Library Entry** | A reusable drill record. Discriminated by `source` field (`system` or `user`). |
| **My Drills** | The coach's personal library. Filter: `source = 'user'`. |
| **Drill Library** | System-curated drills available to all coaches. Filter: `source = 'system'`. |
| **Drill Type** | Classification tag (e.g., Hitting, Fielding, Pitching). |
| **Station** | 2+ Plan Drills running simultaneously. Linked by shared `station_group` UUID. Not a DB entity. |
| **station_group** | Optional UUID on `plan_drill`. Drills sharing the same value form a Station. Null = sequential. |
| **Duration Tracking** | Real-time total: sum of sequential durations + longest drill per Station group. |
| **Coach Assignment** | Free-text field on Plan Drill. Will become FK in future. |
| **Player Count** | Integer on Plan Drill. Placeholder for future player assignment feature. |
| **Demonstration Link** | URL (typically YouTube) for drill video. Stored as `demo_link`. |
| **Save as Template** | Copies a Plan Drill's data into a new My Drill entry. Independent copies after save. |
| **Plan Export** | PDF/printable output. Presentation layer only — no data model changes. |
| **Participant** | Future entity. Non-coach user (assistant_coach, parent, player). NOT in MVP. |
| **Position Group** | Future concept. Named grouping of players by defensive position. NOT in MVP. |
