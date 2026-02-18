# ChessStudy API Specification

## Overview

The ChessStudy API supports:

- Users
- Chess Files (studies)
- Position trees (move variations)
- Notes
- Arrows

All endpoints are prefixed with:

```
/api
```

---

# 1️⃣ Authentication (Future Implementation)

## POST `/api/auth/register`

Creates a new user.

### Request Body

```json
{
  "email": "string",
  "password": "string"
}
```

### Response

```json
{
  "userId": 1,
  "email": "string"
}
```

---

## POST `/api/auth/login`

Authenticates a user.

### Request Body

```json
{
  "email": "string",
  "password": "string"
}
```

### Response

```json
{
  "token": "jwt-token"
}
```

---

# 2️⃣ Chess Files

## GET `/api/chessfiles?userId=1`

Returns all files belonging to a user.

### Response

```json
[
  {
    "chessFileId": 1,
    "name": "Queen's Gambit",
    "description": "Dev file"
  }
]
```

---

## GET `/api/chessfiles/{fileId}`

Returns a single chess file.

---

## POST `/api/chessfiles`

Creates a new chess file and root position.

### Request Body

```json
{
  "name": "string",
  "description": "string",
  "rootFen": "optional string"
}
```

### Behavior

- Creates `ChessFile`
- Creates root `Position`
  - `ParentPositionId = null`
  - `Ply = 0`
  - `MoveUci = null`
  - `MoveSan = null`

---

## PATCH `/api/chessfiles/{fileId}`

Updates file metadata.

### Request Body

```json
{
  "name": "string",
  "description": "string"
}
```

---

## DELETE `/api/chessfiles/{fileId}`

Deletes a chess file.

(Delete behavior must be explicitly defined: restrict or cascade.)

---

# 3️⃣ Positions (Move Tree)

## GET `/api/positions/tree?fileId=1`

Returns full move tree for a file.

### Response (PositionNode DTO)

```json
{
  "positionId": 1,
  "parentPositionId": null,
  "fen": "string",
  "moveUci": null,
  "moveSan": null,
  "ply": 0,
  "siblingOrder": 0,
  "children": []
}
```

---

## POST `/api/positions`

Creates a new child position.

### Request Body

```json
{
  "parentPositionId": 1,
  "moveUci": "e2e4"
}
```

### Behavior

- Load parent position
- Compute:
  - New FEN
  - SAN
  - Ply = parent.Ply + 1
  - SiblingOrder
- Save new Position

---

## PATCH `/api/positions/{parentId}/reorder`

Reorders sibling positions.

### Request Body

```json
[5, 3, 8]
```

Updates `SiblingOrder` for each child.

---

## DELETE `/api/positions/{positionId}`

Deletes a position.

Policy decision required:
- Restrict if children exist
- OR delete subtree explicitly

Optional future endpoint:

```
DELETE /api/positions/{positionId}/subtree
```

---

# 4️⃣ Notes

## GET `/api/positions/{positionId}/notes`

Returns notes for a position.

---

## POST `/api/positions/{positionId}/notes`

### Request Body

```json
{
  "text": "string"
}
```

---

## PATCH `/api/notes/{noteId}`

Updates note content.

---

## DELETE `/api/notes/{noteId}`

Deletes a note.

---

# 5️⃣ Arrows

## GET `/api/positions/{positionId}/arrows`

Returns arrows for a position.

---

## POST `/api/positions/{positionId}/arrows`

### Request Body

```json
{
  "fromSquare": "e2",
  "toSquare": "e4",
  "color": "green"
}
```

---

## DELETE `/api/arrows/{arrowId}`

Deletes an arrow.

---

# Data Relationships

```
User (1) → (many) ChessFiles
ChessFile (1) → (many) Positions
Position (1) → (many) ChildPositions
Position (1) → (many) Notes
Position (1) → (many) Arrows
```

---

# Domain Rules

- Each `ChessFile` must have exactly one root `Position`.
- Root Position:
  - `ParentPositionId = null`
  - `Ply = 0`
  - `MoveUci = null`
  - `MoveSan = null`
- Self-referencing `Position` relationships use `DeleteBehavior.Restrict`.
- SAN may be stored or derived.
- Ply should be computed server-side.

---

# Architectural Notes

- Entities (`Models/`) represent database structure.
- DTOs (`DTOs/`) represent API response shape.
- Controllers handle HTTP.
- Services (future) contain business logic.
- `AppDbContext` manages persistence.

---

# Future Enhancements

- JWT Authentication
- Role-based authorization
- Caching move trees
- Engine validation for moves
- Soft deletes instead of hard deletes
