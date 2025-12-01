const API_BASE = 'http://localhost:5053/api';

const handleResponse = async (response) => {
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || 'Request failed');
  }
  if (response.status === 204) {
    return null;
  }
  return response.json();
};

export const createShelf = async (payload) => {
  const response = await fetch(`${API_BASE}/Shelves/CreateShelf`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
  return handleResponse(response);
};

export const authenticateShelf = async (payload) => {
  const response = await fetch(`${API_BASE}/Shelves/AuthenticateShelf`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
  return handleResponse(response);
};

export const addBook = async (payload) => {
  const response = await fetch(`${API_BASE}/Books/AddBook`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
  return handleResponse(response);
};

export const getBooks = async (shelfId) => {
  const params = new URLSearchParams({ shelfId: String(shelfId) });
  const response = await fetch(`${API_BASE}/Books/GetBooksForShelf?${params}`);
  return handleResponse(response);
};

export const removeBook = async ({ shelfId, bookId }) => {
  const response = await fetch(`${API_BASE}/Books/RemoveBook`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ shelfId, bookId })
  });
  return handleResponse(response);
};

export const searchOpenLibrary = async ({ query, title, author, series, isbn, page = 1, limit = 100 }) => {
  const url = new URL('https://openlibrary.org/search.json');
  const qParts = [];
  if (title) qParts.push(`title:${title}`);
  if (author) qParts.push(`author:${author}`);
  if (series) qParts.push(`series:${series}`);
  if (query) qParts.push(query);
  if (isbn) qParts.push(`isbn:${isbn}`);

  if (qParts.length === 0) {
    throw new Error('Enter at least one search field.');
  }

  url.searchParams.set('q', qParts.join(' '));
  url.searchParams.set('limit', String(limit));
  url.searchParams.set('page', String(page));
  url.searchParams.set('fields', 'key,title,author_name,isbn,first_publish_year,series');

  const response = await fetch(url);
  if (!response.ok) {
    throw new Error('Open Library search failed');
  }

  const data = await response.json();
  return Array.isArray(data.docs) ? data.docs : [];
};
