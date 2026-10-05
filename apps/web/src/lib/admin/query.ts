export const DEFAULT_PAGE_SIZE = 12;
export const MAX_PAGE_SIZE = 100;

export function readPage(value: string | null): number {
  const parsed = Number(value ?? "1");
  if (!Number.isFinite(parsed) || parsed < 1) {
    return 1;
  }

  return Math.floor(parsed);
}

export function readPageSize(value: string | null): number {
  const parsed = Number(value ?? String(DEFAULT_PAGE_SIZE));
  if (!Number.isFinite(parsed) || parsed < 1) {
    return DEFAULT_PAGE_SIZE;
  }

  return Math.min(MAX_PAGE_SIZE, Math.floor(parsed));
}

export function buildQuery(params: Record<string, string | number | undefined | null>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null) {
      continue;
    }

    const text = String(value).trim();
    if (!text) {
      continue;
    }

    search.set(key, text);
  }

  const query = search.toString();
  return query ? `?${query}` : "";
}

export function withListQuery(path: string, params: Record<string, string | number | undefined | null>): string {
  return `${path}${buildQuery(params)}`;
}
