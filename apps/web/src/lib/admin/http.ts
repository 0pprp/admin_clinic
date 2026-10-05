import { ApiRequestError, apiFetch, readApiError } from "@/lib/api/client";

export async function adminJson<T>(path: string, fallback: string, init?: RequestInit): Promise<T> {
  const response = await apiFetch(path, init);
  if (!response.ok) {
    throw new ApiRequestError(await readApiError(response, fallback), response.status, null);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  if (!text) {
    return undefined as T;
  }

  return JSON.parse(text) as T;
}

export async function adminVoid(path: string, fallback: string, init?: RequestInit): Promise<void> {
  await adminJson<void>(path, fallback, init);
}

export function errorMessage(caught: unknown, fallback: string): string {
  return caught instanceof ApiRequestError ? caught.message : fallback;
}

export function isForbidden(caught: unknown): boolean {
  return caught instanceof ApiRequestError && caught.status === 403;
}

export function isUnauthorized(caught: unknown): boolean {
  return caught instanceof ApiRequestError && caught.status === 401;
}

export function isNotFound(caught: unknown): boolean {
  return caught instanceof ApiRequestError && caught.status === 404;
}
