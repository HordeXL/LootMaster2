# LootMaster 2.0 — User Guide

## First Launch

**Step 1 — Open databases**

Two separate databases are supported:

| Button | Database | Contains |
|---|---|---|
| **"Open SQLite DB"** | `compact.sqlite3` | items, npcs, item_categories, localized_texts |
| **"Open Loot DB"** | `compact.server.table.sqlite3` | loots, loot_groups, loot_pack_dropping_npcs |

If the loot DB is not selected, loot tables are looked up in the main database (backwards compatibility).
Both paths are saved in the progress file and restored automatically.

**Step 2 — Open loot JSON files**
Click **"Open Drop JSON"**. You can select multiple files at once (e.g. `loot1.json`, `loot_doodad.json`).

Two formats are supported:

NPC loot:
```json
[ { "npc_id": 123, "items": [ { "item_id": 456 }, ... ] }, ... ]
```
Doodad loot (world objects):
```json
[ { "doodad_id": 322, "loot_pack_id": 6414, "items": [ { "item_id": 456, "name": "...", "count": 1 }, ... ] }, ... ]
```
Both formats can be loaded simultaneously. `loot_pack_id` from doodad files is used directly.
After loading, the status bar shows statistics: how many items, categories, and NPCs were loaded.

> On subsequent launches the session is restored automatically — no need to reselect files.  
> If a JSON file has been moved or deleted it is silently excluded from the session.  
> Files that are not JSON arrays (e.g. the progress file) are skipped with a warning.

---

## Interface

```
┌─ Toolbar ────────────────────────────────────────────────────────┐
│ [Open SQLite DB] [Open Loot DB] | [Open Drop JSON]              │
│ | [Save Progress] [Export Result] | [Write to DB] | [Import SQL] │
│ | [Search...] [Unprocessed only] [Highlight categories]          │
│ [Show NPC ID] [Show NPC names] | [RU/EN/ZH]                      │
├──────────────────────────────────────────────────────────────────┤
├─ Blue badges with names of loaded JSON files ─────────────────────┤
│                                                                   │
│  ◄ ITEM TABLE (60%) ──────────►│◄── RIGHT PANEL (40%) ►         │
│                                │                                  │
│  All items with drop data      │  • Selected item (info)         │
│  Click a row to select         │  • Item settings                │
│                                │  • Category settings            │
│                                │  • Navigation                   │
│                                │  • NPCs of selected item        │
│                                │  • Summary (statistics)         │
└────────────────────────────────┴──────────────────────────────────┘
│ Status bar (current state / loading spinner)                       │
```

**Item table columns:**

| Column | Description |
|---|---|
| Item ID | Item ID |
| Item | Localized name |
| Cat ID / Category | Category ID and name |
| NPC ID / NPC | NPC IDs and names (hidden by default, toggled via checkboxes) |
| Group (item) / Chance (item) | Manually assigned item-level values |
| Group (cat) / Chance (cat) | Values inherited from the category |
| Eff. group / Eff. chance | Final values (item takes priority over category) |
| Loot Pack ID | Loot pack ID linked to the item's NPC |
| Group (DB) / Chance (DB) | Values already written to the `loots` table in the database |
| Min Qty / Max Qty | Manually assigned min/max drop quantity |
| Grade ID | Assigned item grade ID (effective: item-level → DB fallback) |
| Always | "Always drop" flag (effective: item-level → DB fallback) |

**Row colors:**
- **Green** — a group is assigned directly to the item (item-level)
- **Yellow** — group inherited from the category (category-level), when category highlighting is enabled
- **White** — unprocessed

The selected row is always shown in blue — even when focus moves to the input fields in the right panel.

> **Column layout, panel widths, and window size** are saved automatically between sessions (`Data\column-settings.json`).

---

## Core Workflow — Assigning Loot Groups

### Item

1. Click an item in the table on the left
2. In the right panel — **"Item Settings"** section
3. Fields are pre-filled automatically: first from saved progress, then from DB values, otherwise defaults
4. Available fields:
   - **Group** and **Chance** — core loot parameters
   - **Min amount** / **Max amount** — drop quantity range
   - **Grade ID** — item grade ID
   - **Always drop** — unconditional drop flag
5. Enter the desired values and click **"Apply"** — the row turns green
6. To reset all item fields — **"Reset"**

**"Apply DB to all"** — batch operation: sets Group, Chance, Grade ID and Always Drop from DB as item-level values for all items that have DB data but no manually set item-level group.

**"Undo DB apply"** — reverses "Apply DB to all": resets group and chance for all items whose item-level group matches Group (DB).

### Category (batch assignment)

1. Select any item from the desired category
2. In the **"Category Settings"** section, enter group, chance, Grade ID and Always drop
3. Click **"Apply to whole category"** — all items in the category turn yellow; Grade ID and Always are applied directly at item-level
4. To reset the category — **"Reset"** (also clears Grade and Always on all items in the category)
5. If an individual item already has its own setting, it takes **priority** over the category

> **Priority rule:** `item group > category group`, `item chance > category chance`

### How Loot Groups Work

The **"?"** button in the "Item Settings" header opens a help window inside the app.

| Group | Behaviour |
|---|---|
| **0** | Each item is checked **independently** by its own chance. Multiple items can drop simultaneously. |
| **1, 2, 3...** | "Loot bag": first a roll against `loot_groups.drop_rate` — does the bag open? If yes, **one** item is selected from the group (weighted by `loots.drop_rate`). |

**Real DB example (pack_id=9327):**
- `loot_groups`: group_no=1, drop_rate=41 666 → **0.42%** chance to open the bag
- `loots`: 952 items in group=1 with varying weights
- Result: on NPC kill — first 0.42% to open, then exactly **one** item drops from 952

**Multiple groups per pack_id** — each group is checked independently. One item drops from each triggered group.

**Formula:** `drop_rate ÷ 10 000 000 = chance`. Example: 500 000 → 5%

---

## Filters and Search

| Element | Action |
|---|---|
| **"Search:"** field | Filters by ID, item name, category ID/name, NPC ID/name |
| **"Unprocessed only"** checkbox | Hides already processed items (green and yellow rows) |
| **"Highlight categories"** checkbox | Toggles yellow highlighting for rows that inherit group from category |
| **"Show NPC ID"** / **"Show NPC names"** checkboxes | Shows/hides the corresponding table columns |
| **"RU"** / **"EN"** / **"ZH"** button | Switches the entire UI language. Setting is saved between sessions. |

---

## Navigation

| Button | Shortcut | Action |
|---|---|---|
| **Previous** | — | Previous item in the filtered list |
| **Next** | — | Next item |
| **Next unprocessed** | — | Jump to the nearest item without an assigned group |
| Save progress | **Ctrl+S** | Explicit save |

After navigation buttons are clicked, focus returns to the main table automatically and the selected row scrolls into view.

---

## NPC Browser

**"NPCs of Selected Item"** section in the right panel:

- **NPC list** — all NPCs that drop the selected item. The count is shown in parentheses in the header. Clicking an NPC updates the table below.
- **"All items of selected NPC"** table — all items of the selected NPC with current groups/chances and Loot Pack ID. The row count is shown in parentheses in the header.
  - **Single click** — focus moves to the main table
  - **Double click** — navigates to that item in the main table (filters are cleared automatically)

---

## Saving and Export

| Action | Result |
|---|---|
| **Ctrl+S** / "Save Progress" | Saves the working file to the `Data\` folder next to the exe |
| Auto-save | Happens automatically after every Apply / Reset |
| **"Export Result"** | Saves the final JSON: `{ "items": [ { "item_id": X, "chance": Y }, ... ] }` |
| **"Write to DB"** | Writes all assigned values directly to SQLite: upsert of `loots` and `loot_groups`. Shows a preview (how many rows will be updated/inserted) before writing. |
| **"Import SQL to DB"** | Executes SQL file(s) directly into the database. Multiple files can be selected. Supports standard SQL and Navicat dumps (INSERT without column names). New rows are inserted, existing rows (by `id`) are updated without deletion. |
| **"Generate Patch from DB"** | Reads `loot_actability_groups`, `loot_groups`, `loot_pack_dropping_npcs`, `loots` from a selected SQLite database and generates an `INSERT OR REPLACE` SQL patch in the `Data\` folder. Shows a preview with per-table row counts and the file path. The **target DB is not modified** — apply the patch manually via "Import SQL to DB". |

### Import Statistics

The **"Summary"** section shows cumulative statistics for both import types side by side:

```
── SQL Import ──        ── Patch Gen ──
Inserted:  N            Inserted:  N
Updated:   M            Updated:   M
Total:     N+M          Total:     N+M
```

Statistics accumulate across sessions and are saved in the progress file.

---

## File Locations

All data is stored **next to the executable** in the `Data\` folder:

```
LootMaster.exe
Data\
  loot_group_progress.json      ← working progress file (includes import statistics)
  column-settings.json          ← column layout, panel widths, window size, language
  patch_YYYYMMDD_HHMMSS.sql     ← patch files generated via "Generate Patch from DB"
```

---

## File Formats

### Progress file (`Data\loot_group_progress.json`)
```json
{
  "db_path": "C:/path/to/compact.sqlite3",
  "loot_db_path": "C:/path/to/compact.server.table.sqlite3",
  "source_json_paths": ["C:/path/to/loot1.json", "C:/path/to/loot_doodad.json"],
  "items": {
    "12345": { "group": 1, "chance": 0.5, "min_amount": 1, "max_amount": 3, "grade_id": 2, "always_drop": false }
  },
  "categories": {
    "100": { "group": 2, "chance": 1.0, "grade_id": 1, "always_drop": false }
  },
  "sql_imports": { "patch.sql": { "inserted": 100, "updated": 50 } },
  "db_imports":  { "source.sqlite3": { "inserted": 4109, "updated": 0 } }
}
```

### Input loot JSON (NPC)
```json
[
  {
    "npc_id": 123,
    "items": [
      { "item_id": 456 },
      { "item_id": 789 }
    ]
  }
]
```

### Input loot JSON (Doodad)
```json
[
  {
    "doodad_id": 322,
    "loot_pack_id": 6414,
    "items": [
      { "item_id": 456, "name": "Some item", "count": 1 }
    ]
  }
]
```

### Export JSON
```json
{
  "items": [
    { "item_id": 456, "chance": 0.5 },
    { "item_id": 789, "chance": 1.0 }
  ]
}
```

---

## Typical Workflow

```
1. Open DB + JSON (once; restored automatically on next launch)
2. Enable "Unprocessed only"
3. Process items one by one:
   - If the whole category is the same → "Apply to whole category"
   - If an item is special → "Apply" only to it
4. Use the NPC browser to see drop context
   - "Group (DB)" and "Chance (DB)" columns show what is already in the database
   - Loot Pack ID helps identify which loot pool the item belongs to
5. Ctrl+S periodically
6. When done:
   - "Write to DB" — write directly to SQLite (upsert loots + loot_groups)
   - or "Export Result" — save JSON for further processing
   - or "Import SQL" — load an SQL dump directly into the loot DB
   - or "Generate Patch from DB" — generate a patch from another SQLite database, then apply via "Import SQL to DB"
```
