export const STAFF_ROLES = ["Admin", "Support", "ContentManager"] as const;

export type StaffRole = (typeof STAFF_ROLES)[number];

export function hasRole(roles: string[], role: string): boolean {
  return roles.includes(role);
}

export function isStaff(roles: string[]): boolean {
  return STAFF_ROLES.some((role) => roles.includes(role));
}

export function isAdmin(roles: string[]): boolean {
  return roles.includes("Admin");
}

export function canManagePayments(roles: string[]): boolean {
  return roles.includes("Admin") || roles.includes("Support");
}

export function canManageActivations(roles: string[]): boolean {
  return roles.includes("Admin") || roles.includes("Support");
}

export function canManageStudents(roles: string[]): boolean {
  return roles.includes("Admin") || roles.includes("Support");
}

export function canManageCourses(roles: string[]): boolean {
  return roles.includes("Admin") || roles.includes("ContentManager");
}

export function canManageContent(roles: string[]): boolean {
  return roles.includes("Admin") || roles.includes("ContentManager");
}

export function canManageConsultations(roles: string[]): boolean {
  return roles.includes("Admin") || roles.includes("Support");
}

export function canManageUsers(roles: string[]): boolean {
  return roles.includes("Admin");
}

export function canViewAuditLogs(roles: string[]): boolean {
  return roles.includes("Admin");
}
