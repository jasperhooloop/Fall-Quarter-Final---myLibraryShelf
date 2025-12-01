# 📚 myLibraryShelf

A Windows XP-themed personal library manager built with ASP.NET Core Web API, SQL Server, and a Vite + React frontend. Create or load a shelf, search Open Library for books, add/remove titles, and keep a tidier catalog.

---

## 🚀 Quick Start

1. **Database** (SQL Server 2022 / Local):
   ```sh
   sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -i sql/create_database.sql
   sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -i sql/create_tables.sql
   sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -i sql/stored_procedures.sql
   ```
2. **Backend**:
   ```sh
   cd backend/MyLibraryShelf.Api
   dotnet restore
   dotnet run # default: http://localhost:5053
   ```
3. **Frontend**:
   ```sh
   cd frontend
   npm install
   npm run dev # default: http://localhost:5173
   ```
4. Open the frontend site → create a shelf → search for books → add/remove titles.

---

## 🧰 Stack

| Layer      | Tech / Notes |
|-----------|--------------|
| Backend   | ASP.NET Core Web API (.NET 9), controllers for shelves/books/search history |
| Data      | SQL Server (scripts + stored procedures). `DatabaseService` uses `SqlConnection` |
| Frontend  | React (Vite), Windows XP-inspired UI with custom icons & scrollbar |
| API       | Open Library Search API (https://openlibrary.org/search.json) |

---

## 🗂️ Project Structure

```
myLibraryShelf/
├── README.md
├── backend/
│   └── MyLibraryShelf.Api/
│       ├── Controllers/
│       ├── Models/
│       ├── Data/
│       ├── sql/ (database scripts)
│       ├── Program.cs, appsettings*.json, MyLibraryShelf.Api.csproj
├── frontend/
│   ├── src/ (React app)
│   ├── public/
│   ├── package.json, vite.config.js, README notes
├── sql/
│   ├── create_database.sql
│   ├── create_tables.sql
│   ├── stored_procedures.sql
│   └── myTestQueries.sql
└── assets/
    ├── Background.jpg, window.png, button icons, etc.
```

---

## 🗄️ Database Scripts

1. `create_database.sql`: creates the `MyLibraryShelf` DB.
2. `create_tables.sql`: builds **Shelves**, **Books**, **SearchHistory** tables with FK relations.
3. `stored_procedures.sql`: stored procedures for creating shelves, authenticating, adding/removing books, recording search history, retrieving paginated book/audiobook views.
4. `myTestQueries.sql`: quick diagnostics (list databases, procs, tables).

> Default connection string (see `appsettings*.json`):
> `Server=localhost,1433;Database=MyLibraryShelf;User ID=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;`

---

## 🎛️ Backend Highlights

- .NET 9 minimal hosting with Swagger enabled in Development.
- Controllers:
  - `ShelvesController`: Create + authenticate shelf entries.
  - `BooksController`: Add books, get book/audiobook lists, remove entries.
  - `SearchHistoryController`: Logs search terms.
- `DatabaseService`: centralized `SqlConnection` factory.
- `StoredProcedures`: `sp_CreateShelf`, `sp_AuthenticateShelf`, `sp_AddBook`, `sp_RemoveBook`, `sp_GetBooksForShelf`, `sp_GetAudiobooksForShelf`, `sp_AddSearchHistory`.

Build/run again:
```sh
cd backend/MyLibraryShelf.Api
dotnet restore
dotnet run
```

---

## 🖥️ Frontend Highlights

- Vite + React app styled after Windows XP: blue frame, icon buttons, custom scrollbar.
- Features:
  - Create/load shelf forms.
  - Search form hitting Open Library (Title / Author / Series / Keywords / ISBN).
  - Fixed-height search results area with a custom scroll track/handle.
  - Shelf view with selections + bulk remove.
- Config: Update `frontend/src/services/api.js` if backend URL changes.

Run:
```sh
cd frontend
npm install
npm run dev
```

---

## 🌐 API Usage

- React app calls backend endpoints at `http://localhost:5053/api/...` (proxied in dev when needed).
- Backend stored procedures handle all DB interactions for consistency, ensuring shelf/business logic stays server-side.
- Open Library Search API (https://openlibrary.org/search.json) supplies book metadata; user queries (title/author/series/ISBN) are forwarded, and the UI displays the results with a custom scrollbar for easy browsing.

---

## 🧪 Verification

Use `sql/myTestQueries.sql` to confirm:
- Database exists
- Current DB context
- Stored procedures/tables present
- SQL Server version

```sh
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -i sql/myTestQueries.sql
```

Then test end-to-end: create shelf → search → add book → verify shelf state → remove selection.

---

## 🙌 Credits / Notes

- UI assets located in `/assets` (icons, backgrounds, window chrome). 
- Open Library API docs: https://openlibrary.org/developers/api
- For SQL Server in Docker, update `appsettings*.json` and connection strings as needed (e.g., port 1533).
- Keep `docker-compose.yml` (Azure SQL Edge) handy if you prefer LocalDB alternatives.

Happy cataloging! 🪟✨
