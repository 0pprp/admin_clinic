export type AdminDashboardSummary = {
  pendingPurchaseRequests: number;
  awaitingPaymentRequests: number;
  paymentReceivedRequests: number;
  activeStudents: number;
  activeEnrollments: number;
  publishedCourses: number;
  newConsultations: number;
  unreadContactMessages: number;
  recentPurchaseRequests: AdminPurchaseLite[];
  recentActivations: AdminActivationLite[];
  recentConsultations: AdminConsultationSummary[];
  recentActivity: AdminAuditLogSummary[];
};

export type AdminPurchaseLite = {
  id: string;
  requestNumber: string;
  fullName: string;
  courseTitle: string;
  status: string;
  createdAt: string;
};

export type AdminActivationLite = {
  id: string;
  userFullName: string;
  courseTitle: string;
  status: string;
  createdAt: string;
};

export type AdminPurchaseSummary = {
  id: string;
  requestNumber: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  courseTitle: string;
  amountIQD: number;
  status: string;
  createdAt: string;
};

export type AdminPurchaseEvent = {
  id: string;
  fromStatus: string | null;
  toStatus: string;
  actorUserId: string | null;
  note: string | null;
  createdAt: string;
};

export type AdminPurchaseDetail = {
  id: string;
  requestNumber: string;
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  whatsAppNumber: string | null;
  governorate: string | null;
  courseId: string;
  courseTitle: string;
  courseSlug: string;
  amountIQD: number;
  paymentMethod: string | null;
  paymentReference: string | null;
  status: string;
  customerNotes: string | null;
  adminNotes: string | null;
  createdAt: string;
  contactedAt: string | null;
  paymentReceivedAt: string | null;
  confirmedBy: string | null;
  canActivate: boolean;
  timeline: AdminPurchaseEvent[];
};

export type IssuedActivationCode = {
  activationCode: string;
  expiresAt: string;
  requestNumber: string;
};

export type DirectActivationResult = {
  enrollmentId: string;
  courseId: string;
  courseTitle: string;
  courseSlug: string;
  status: string;
  requestNumber: string;
};

export type AdminStudentSummary = {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  whatsAppNumber: string | null;
  accountStatus: string;
  roles: string[];
  createdAt: string;
  lastLoginAt: string | null;
};

export type AdminStudentPurchase = {
  id: string;
  requestNumber: string;
  courseTitle: string;
  amountIQD: number;
  status: string;
  createdAt: string;
};

export type AdminStudentEnrollment = {
  id: string;
  courseId: string;
  courseTitle: string;
  courseSlug: string;
  status: string;
  startedAt: string;
  expiresAt: string | null;
  canAccess: boolean;
  progressPercent: number;
  completedLessons: number;
  totalLessons: number;
};

export type AdminStudentDetail = {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  whatsAppNumber: string | null;
  governorate: string | null;
  accountStatus: string;
  roles: string[];
  createdAt: string;
  lastLoginAt: string | null;
  purchaseRequests: AdminStudentPurchase[];
  enrollments: AdminStudentEnrollment[];
};

export type AdminEnrollmentSummary = {
  id: string;
  userId: string;
  studentName: string;
  studentEmail: string;
  courseId: string;
  courseTitle: string;
  status: string;
  startedAt: string;
  expiresAt: string | null;
  activationMethod: string;
  purchaseRequestNumber: string | null;
};

export type AdminCourseSummary = {
  id: string;
  title: string;
  slug: string;
  status: string;
  level: string;
  priceIQD: number;
  isFeatured: boolean;
  sectionCount: number;
  lessonCount: number;
  updatedAt: string;
};

export type AdminLesson = {
  id: string;
  sectionId: string;
  title: string;
  description: string | null;
  durationSeconds: number;
  sortOrder: number;
  isFreePreview: boolean;
  status: string;
  videoProvider: string;
  videoKey: string | null;
  videoProcessingStatus?: string | null;
  videoProcessingMessage?: string | null;
};

export type AdminLessonVideoStatus = {
  lessonId: string;
  videoProvider: string;
  videoKey: string | null;
  processingStatus: string;
  message: string | null;
  durationSeconds: number;
};

export type AdminSection = {
  id: string;
  courseId: string;
  title: string;
  description: string | null;
  sortOrder: number;
  lessons: AdminLesson[];
};

export type AdminCourseDetail = {
  id: string;
  title: string;
  slug: string;
  shortDescription: string;
  description: string;
  priceIQD: number;
  thumbnailUrl: string | null;
  trailerUrl: string | null;
  level: string;
  status: string;
  isFeatured: boolean;
  accessType: string;
  accessDurationDays: number | null;
  createdAt: string;
  updatedAt: string;
  sections: AdminSection[];
};

export type SaveCourseRequest = {
  title: string;
  slug: string;
  shortDescription: string;
  description: string;
  priceIQD: number;
  thumbnailUrl: string | null;
  trailerUrl: string | null;
  level: string;
  accessType: string;
  accessDurationDays: number | null;
  isFeatured: boolean;
};

export type AdminActivationCodeSummary = {
  id: string;
  userId: string;
  userEmail: string;
  userFullName: string;
  courseId: string;
  courseTitle: string;
  purchaseRequestId: string;
  requestNumber: string;
  status: string;
  createdAt: string;
  expiresAt: string | null;
  usedAt: string | null;
  createdBy: string;
};

export type AdminConsultationSummary = {
  id: string;
  requestNumber: string;
  fullName: string;
  phoneNumber: string;
  consultationType: string;
  preferredDate: string | null;
  status: string;
  createdAt: string;
};

export type AdminConsultationDetail = {
  id: string;
  requestNumber: string;
  userId: string | null;
  fullName: string;
  phoneNumber: string;
  whatsAppNumber: string | null;
  email: string | null;
  consultationType: string;
  companyName: string | null;
  preferredDate: string | null;
  preferredTime: string | null;
  topic: string;
  message: string;
  preferredCommunicationMethod: string;
  status: string;
  scheduledAt: string | null;
  adminNotes: string | null;
  createdAt: string;
  updatedAt: string;
};

export type AdminArticleSummary = {
  id: string;
  title: string;
  slug: string;
  status: string;
  publishedAt: string | null;
  updatedAt: string;
};

export type AdminArticleDetail = {
  id: string;
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  coverImage: string | null;
  status: string;
  publishedAt: string | null;
  authorId: string;
  createdAt: string;
  updatedAt: string;
};

export type AdminFaq = {
  id: string;
  question: string;
  answer: string;
  sortOrder: number;
  isActive: boolean;
  showOnHome: boolean;
  updatedAt: string;
};

export type AdminContactSummary = {
  id: string;
  name: string;
  email: string;
  subject: string;
  status: string;
  createdAt: string;
};

export type AdminContactDetail = {
  id: string;
  userId: string | null;
  name: string;
  phone: string | null;
  email: string;
  subject: string;
  message: string;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type AdminAuditLogSummary = {
  id: string;
  createdAt: string;
  adminUserId: string;
  actorName: string;
  action: string;
  entityType: string;
  entityId: string | null;
  description: string;
};

export type AdminAuditLogDetail = AdminAuditLogSummary & {
  metadataJson: string | null;
  ipAddress: string | null;
};

export type AdminSiteSettings = {
  brandName: string | null;
  brandNameEnglish: string | null;
  publicPhone: string | null;
  publicWhatsApp: string | null;
  publicEmail: string | null;
  socialLinks: string | null;
  footerText: string | null;
  consultationInfo: string | null;
  paymentMethods?: string | null;
  transferInstructions?: string | null;
  supportPhone?: string | null;
  supportWhatsApp?: string | null;
};

export type AdminExpertise = {
  id: string;
  title: string;
  description: string;
  iconKey: string | null;
  sortOrder: number;
  isActive: boolean;
  updatedAt: string;
};

export type AdminTestimonial = {
  id: string;
  authorDisplayName: string;
  authorTitle: string | null;
  body: string;
  isPublished: boolean;
  sortOrder: number;
  updatedAt: string;
};

export type AdminStatistic = {
  id: string;
  key: string;
  label: string;
  displayValue: string;
  isPlaceholder: boolean;
  sortOrder: number;
  isActive: boolean;
  updatedAt: string;
};

export type CreateConsultationResponse = {
  id: string;
  requestNumber: string;
  status: string;
};

export type CreateContactResponse = {
  id: string;
  status: string;
};

export type ReorderItem = {
  id: string;
  sortOrder: number;
};
