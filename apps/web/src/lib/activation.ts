export type RedeemActivationResponse = {
  enrollmentId: string;
  courseId: string;
  courseTitle: string;
  courseSlug: string;
  status: string;
  expiresAt: string | null;
};

export function normalizeActivationInput(value: string): string {
  return value.replace(/[\s_-]+/g, "").toUpperCase();
}
