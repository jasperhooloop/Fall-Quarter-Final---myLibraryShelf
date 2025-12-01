import { useEffect, useState } from 'react';
import {
  createShelf,
  authenticateShelf,
  addBook,
  getBooks,
  searchOpenLibrary,
  removeBook
} from './services/api';

const initialSearchForm = {
  title: '',
  author: '',
  series: '',
  query: '',
  isbn: ''
};

const ICONS = {
  create: '/Create%20Shelf.png',
  load: '/LoadShelf.png',
  search: '/Search.png',
  add: '/Add.png',
  remove: '/Delete.png'
};

function IconButton({ icon, children, className = '', ...rest }) {
  return (
    <button className={`icon-button ${className}`.trim()} {...rest}>
      <img src={icon} alt="" className="btn-icon" />
      <span>{children}</span>
    </button>
  );
}

function App() {
  const [createShelfForm, setCreateShelfForm] = useState({ shelfName: '', passwordHash: '' });
  const [loginForm, setLoginForm] = useState({ shelfName: '', passwordHash: '' });
  const [searchForm, setSearchForm] = useState(initialSearchForm);
  const [searchResults, setSearchResults] = useState([]);
  const [books, setBooks] = useState([]);
  const [selectedBookIds, setSelectedBookIds] = useState([]);
  const [message, setMessage] = useState('');
  const [isSearching, setIsSearching] = useState(false);
  const [activeShelf, setActiveShelf] = useState(null);

  useEffect(() => {
    if (activeShelf) {
      fetchBooksForShelf(activeShelf.id).catch(() => {
        setBooks([]);
        setMessage('Unable to load books for this shelf.');
      });
    } else {
      setBooks([]);
    }
    setSelectedBookIds([]);
  }, [activeShelf]);

  const fetchBooksForShelf = async (shelfId) => {
    const data = await getBooks(shelfId);
    setBooks(data);
    setSelectedBookIds([]);
  };

  const handleCreateShelf = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      const response = await createShelf(createShelfForm);
      const newShelf = { id: response.shelfId, name: createShelfForm.shelfName };
      setActiveShelf(newShelf);
      setMessage(`Shelf "${newShelf.name}" created with ID ${newShelf.id}`);
      setCreateShelfForm({ shelfName: '', passwordHash: '' });
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleLogin = async (event) => {
    event.preventDefault();
    setMessage('');
    try {
      const response = await authenticateShelf(loginForm);
      const shelf = { id: response.shelfId, name: loginForm.shelfName };
      setActiveShelf(shelf);
      setMessage(`Loaded myLibraryShelf "${shelf.name}" (ID ${shelf.id})`);
    } catch (error) {
      setMessage(`Unable to load shelf: ${error.message}`);
    }
  };

  const handleSearch = async (event) => {
    event.preventDefault();
    setMessage('');
    setIsSearching(true);
    try {
      const results = await searchOpenLibrary(searchForm);
      setSearchResults(results);
      if (results.length === 0) {
        setMessage('No books found.');
      }
    } catch (error) {
      setMessage(error.message);
      setSearchResults([]);
    } finally {
      setIsSearching(false);
    }
  };

  const handleAddBookFromSearch = async (doc) => {
    setMessage('');
    if (!activeShelf) {
      setMessage('Create or load a myLibraryShelf before adding books.');
      return;
    }

    const author = Array.isArray(doc.author_name) && doc.author_name.length > 0
      ? doc.author_name[0]
      : 'Unknown';
    const isbn = Array.isArray(doc.isbn) && doc.isbn.length > 0 ? doc.isbn[0] : null;

    try {
      const payload = {
        shelfId: activeShelf.id,
        title: doc.title || 'Untitled',
        author,
        isbn,
        type: 'Book',
        durationMinutes: null,
        narrator: null,
        publishedYear: doc.first_publish_year || null,
        notes: null
      };
      const response = await addBook(payload);
      setMessage(`Added "${payload.title}" (Book ID ${response.bookId}) to ${activeShelf.name}.`);
      await fetchBooksForShelf(activeShelf.id);
    } catch (error) {
      setMessage(`Unable to add book: ${error.message}`);
    }
  };

  const toggleBookSelection = (bookId) => {
    setSelectedBookIds((prev) =>
      prev.includes(bookId) ? prev.filter((id) => id !== bookId) : [...prev, bookId]
    );
  };

  const handleRemoveSelectedBooks = async () => {
    if (!activeShelf || selectedBookIds.length === 0) {
      return;
    }
    try {
      await Promise.all(selectedBookIds.map((bookId) => removeBook({ shelfId: activeShelf.id, bookId })));
      setMessage('Removed selected books from myLibraryShelf.');
      await fetchBooksForShelf(activeShelf.id);
    } catch (error) {
      setMessage(`Unable to remove books: ${error.message}`);
    }
  };

  const statusClass = message.toLowerCase().includes('unable') ? 'error' : 'success';

  return (
    <main className="page">
      <div className="xp-frame">
        <div className="page-inner">
          <div className="hero">
            <h1>myLibraryShelf</h1>
            {message && <p className={statusClass}>{message}</p>}
          </div>

      <div className="split">
        <section>
          <h2>Create a new myLibraryShelf</h2>
          <form onSubmit={handleCreateShelf}>
            <input
              type="text"
              placeholder="Shelf Name"
              value={createShelfForm.shelfName}
              onChange={(e) => setCreateShelfForm({ ...createShelfForm, shelfName: e.target.value })}
              required
            />
            <input
              type="password"
              placeholder="Password"
              value={createShelfForm.passwordHash}
              onChange={(e) => setCreateShelfForm({ ...createShelfForm, passwordHash: e.target.value })}
              required
            />
            <IconButton icon={ICONS.create} type="submit">
              Create Shelf
            </IconButton>
          </form>
        </section>

        <section>
          <h2>Load Existing myLibraryShelf</h2>
          <form onSubmit={handleLogin}>
            <input
              type="text"
              placeholder="Shelf Name"
              value={loginForm.shelfName}
              onChange={(e) => setLoginForm({ ...loginForm, shelfName: e.target.value })}
              required
            />
            <input
              type="password"
              placeholder="Password"
              value={loginForm.passwordHash}
              onChange={(e) => setLoginForm({ ...loginForm, passwordHash: e.target.value })}
              required
            />
            <IconButton icon={ICONS.load} type="submit">
              Load Shelf
            </IconButton>
          </form>
        </section>
      </div>

        <section>
          <h2>Search Open Library</h2>
        <form onSubmit={handleSearch} className="search-form">
          <input
            type="text"
            placeholder="Title"
            value={searchForm.title}
            onChange={(e) => setSearchForm({ ...searchForm, title: e.target.value })}
          />
          <input
            type="text"
            placeholder="Author"
            value={searchForm.author}
            onChange={(e) => setSearchForm({ ...searchForm, author: e.target.value })}
          />
          <input
            type="text"
            placeholder="Series"
            value={searchForm.series}
            onChange={(e) => setSearchForm({ ...searchForm, series: e.target.value })}
          />
          <input
            type="text"
            placeholder="Keyword(s)"
            value={searchForm.query}
            onChange={(e) => setSearchForm({ ...searchForm, query: e.target.value })}
          />
          <input
            type="text"
            placeholder="ISBN"
            value={searchForm.isbn}
            onChange={(e) => setSearchForm({ ...searchForm, isbn: e.target.value })}
          />
          <IconButton icon={ICONS.search} type="submit" disabled={isSearching}>
            {isSearching ? 'Searching...' : 'Search'}
          </IconButton>
        </form>
        <div className="search-results">
          {searchResults.length === 0 && <p>No search results yet.</p>}
          {searchResults.map((doc) => (
            <div className="list-item" key={`book-${doc.key}`}>
              <strong>{doc.title}</strong>
              <div>Author: {Array.isArray(doc.author_name) ? doc.author_name.join(', ') : 'Unknown'}</div>
              <div>First Published: {doc.first_publish_year || 'n/a'}</div>
              <div>ISBN: {Array.isArray(doc.isbn) ? doc.isbn[0] : 'n/a'}</div>
              <IconButton icon={ICONS.add} type="button" onClick={() => handleAddBookFromSearch(doc)}>
                Add to myLibraryShelf
              </IconButton>
            </div>
          ))}
        </div>
        </section>

        <section>
          <h2>What's on myLibraryShelf?</h2>
        {activeShelf ? (
          <p>
            Viewing <strong>{activeShelf.name}</strong> (ID {activeShelf.id})
          </p>
        ) : (
          <p>Create or load a shelf to get started.</p>
        )}
        <div className="lists">
          {books.length === 0 && <p>No books added.</p>}
          {books.map((book) => {
            const isSelected = selectedBookIds.includes(book.bookId);
            return (
              <div
                key={book.bookId}
                className={`list-item selectable ${isSelected ? 'selected' : ''}`}
                onClick={() => toggleBookSelection(book.bookId)}
              >
                <strong>{book.title}</strong>
                <div>Author: {book.author}</div>
                <div>Type: {book.type}</div>
                <div>First Published: {book.publishedYear || 'n/a'}</div>
                <div>ISBN: {book.isbn || 'n/a'}</div>
              </div>
            );
          })}
        </div>
        <IconButton
          icon={ICONS.remove}
          type="button"
          className="danger"
          disabled={!activeShelf || selectedBookIds.length === 0}
          onClick={handleRemoveSelectedBooks}
        >
          Remove from myLibraryShelf
        </IconButton>
      </section>
        </div>
      </div>
    </main>
  );
}

export default App;
