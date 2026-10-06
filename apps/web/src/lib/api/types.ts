export type UserSummary = {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  whatsAppNumber: string | null;
  governorate: string | null;
  roles: string[];
  accountStatus: string;
  emailConfirmed?: boolean;
};

export type ApiError = {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
};

export function firstErrorMessage(error: ApiError, fallback: string): string {
  const fromFields = error.errors
    ? Object.values(error.errors).flat().find((message) => message.trim().length > 0)
    : undefined;

  return error.detail || fromFields || error.title || fallback;
}
