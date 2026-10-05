import { firstErrorMessage, type ApiError } from "./types";

export class ApiRequestError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly body: ApiError | null
  ) {
    super(message);
    this.name = "ApiRequestError";
  }
}

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers);
  headers.set("X-Requested-With", "XMLHttpRequest");

  // FormData must keep browser-generated multipart boundary; never force JSON.
  const isFormData = typeof FormData !== "undefined" && init.body instanceof FormData;
  if (isFormData) {
    headers.delete("Content-Type");
  } else if (init.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  return fetch(path, {
    ...init,
    headers,
    credentials: "include",
    cache: "no-store"
  });
}

/** Upload with progress callback (0-100). Never sets Content-Type (multipart boundary required). */
export function apiUpload(
  path: string,
  formData: FormData,
  onProgress?: (percent: number) => void
): Promise<Response> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("POST", path);
    xhr.withCredentials = true;
    // Intentionally no Content-Type and no custom headers — XHR + FormData must stay multipart.

    xhr.upload.onprogress = (event) => {
      if (!onProgress || !event.lengthComputable || event.total <= 0) {
        return;
      }
      onProgress(Math.min(100, Math.round((event.loaded / event.total) * 100)));
    };

    xhr.onload = () => {
      resolve(
        new Response(xhr.responseText, {
          status: xhr.status,
          statusText: xhr.statusText,
          headers: { "Content-Type": xhr.getResponseHeader("Content-Type") ?? "application/json" }
        })
      );
    };
    xhr.onerror = () => reject(new Error("تعذر رفع الملف بسبب خطأ شبكة."));
    xhr.onabort = () => reject(new Error("تم إلغاء الرفع."));
    xhr.send(formData);
  });
}

export async function readApiError(response: Response, fallback: string): Promise<string> {
  try {
    const body = (await response.json()) as ApiError;
    return firstErrorMessage(body, fallback);
  } catch {
    return fallback;
  }
}

export async function parseJson<T>(response: Response, fallback: string): Promise<T> {
  if (!response.ok) {
    throw new ApiRequestError(await readApiError(response, fallback), response.status, null);
  }

  return (await response.json()) as T;
}
