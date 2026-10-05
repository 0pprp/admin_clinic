function resolveApiOrigin(): string {
  if (process.env.API_INTERNAL_ORIGIN) {
    return process.env.API_INTERNAL_ORIGIN;
  }

  if (process.env.API_PROXY_ORIGIN) {
    return process.env.API_PROXY_ORIGIN;
  }

  // Docker Compose service name in production; local API default otherwise.
  if (process.env.NODE_ENV === "production") {
    return "http://api:8080";
  }

  return "http://127.0.0.1:5000";
}

const API_ORIGIN = resolveApiOrigin();

type FetchPublicOptions = {
  /** Use for pages that must always reflect live catalog/preview state. */
  fresh?: boolean;
};

export async function fetchPublic<T>(path: string, options: FetchPublicOptions = {}): Promise<T | null> {
  try {
    const response = await fetch(`${API_ORIGIN}${path}`, {
      ...(options.fresh
        ? { cache: "no-store" as const }
        : { next: { revalidate: 30 } }),
      headers: { Accept: "application/json" },
      signal: AbortSignal.timeout(8000)
    });

    if (!response.ok) {
      return null;
    }

    return (await response.json()) as T;
  } catch {
    return null;
  }
}

export function normalizeRouteSlug(value: string): string {
  const trimmed = value.trim();
  try {
    return decodeURIComponent(trimmed).normalize("NFC");
  } catch {
    return trimmed.normalize("NFC");
  }
}
