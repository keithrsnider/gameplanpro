# Future State Architecture Notes

These features are NOT being built in MVP. Documented here so current decisions don't make them painful to add later. No additional plumbing is required beyond what is noted.

## MVP Out of Scope

Do NOT build any of these in v1:

- AI-generated practice plans (future paid tier)
- Practice scheduling / calendar / game day events
- Player roster or player database
- Coach roster (coach assignment is free text only)
- Team management UI (team fields on profile only)
- Plan sharing or marketplace features
- Game film integration (e.g., Hudl)
- Communications tools
- Tryout management
- Multi-sport support (baseball only in MVP)
- Mobile app (web only; mobile responsive is future state)
- Coach-to-coach social/community features
- Payments or subscription billing (free beta phase)

## Architecture Decisions for Future Compatibility

### Plan Sharing (Future)

- Coaches will be able to share Practice Plans with Participants who can view but not edit.
- **Do not hardcode single-user scoping on Practice Plan queries.** Plan ownership should be structured so viewer/collaborator access can be added via a permissions layer without refactoring core plan logic.
- **Stable UUID columns (`Uid`) on Practice Plan and Plan Drill records are required** — analytics and sharing features will reference these UUIDs (not the int PKs).

### Participant Entity (Future)

- A Participant is a non-coach user granted access to a Team's resources.
- Role types: `assistant_coach`, `parent`, `player`.
- Lightweight free accounts, similar to TeamSnap/GameChanger model.
- Belongs to a Team; can be granted visibility to shared Practice Plans based on role.
- **No Participant infrastructure needed in MVP.** The `team_name` and `age_group` fields on User profile are the only plumbing required now.

### Player & Position Group Assignment (Future)

- Coaches will assign individual players or position groups to specific drills (e.g., First Basemen, Middle Infielders).
- Will require a many-to-many relationship between Plan Drills and Player Participants.
- **In MVP, `player_count` on Plan Drill (plain integer) is the placeholder.** Do not over-engineer.
- **Do not structure Plan Drills in a way that makes adding a Plan Drill -> Player join table difficult later.**

### Player Progress Analytics (Future)

- Coaches will track player stats over time with AI-generated progress summaries.
- Separate product pillar — does not affect Practice Plan data model.
- The analytics layer will reference Practice Plans and Plan Drills by their persistent UUIDs.
- **Stable UUID columns (`Uid`) on all Plan and Drill records are sufficient preparation.**

### AI Plan Generation (Future)

- Drill library data should be structured to serve as AI training input.
- Keep data clean, consistently categorized, and well-typed.

### Calendar / Scheduling (Future)

- Practice Plans will be assignable to practice events.
- Plans remain templates; events reference plans.

### Multi-Sport (Future)

- Keep a `sport` field on Practice Plans for future expansion (baseball only in MVP).

### White-Labeling (Future)

- Keep branding/theme values in config, not hardcoded, as we may offer white-labeling to organizations.
