export type ExpertiseItem = {
  id: string;
  title: string;
  description: string;
  iconKey: string | null;
  sortOrder: number;
};

export type CourseSummary = {
  id: string;
  title: string;
  slug: string;
  shortDescription: string;
  priceIQD: number;
  thumbnailUrl: string | null;
  level: string;
};

export type CourseLessonSummary = {
  id: string;
  title: string;
  durationSeconds: number;
  sortOrder: number;
  isFreePreview: boolean;
  status: string;
};

export type CourseSectionSummary = {
  id: string;
  title: string;
  description: string | null;
  sortOrder: number;
  lessonCount: number;
  totalDurationSeconds: number;
  lessons: CourseLessonSummary[];
};

export type CourseDetail = CourseSummary & {
  description: string;
  trailerUrl: string | null;
  accessType: string;
  accessDurationDays: number | null;
  sectionCount: number;
  lessonCount: number;
  totalDurationSeconds: number;
  sections: CourseSectionSummary[];
};

export type LessonPreview = {
  lessonId: string;
  title: string;
  description: string | null;
  durationSeconds: number;
  previewAvailable: boolean;
  courseId: string;
  courseSlug: string;
  courseTitle: string;
};

export type ArticleSummary = {
  id: string;
  title: string;
  slug: string;
  excerpt: string;
  coverImage: string | null;
  publishedAt: string | null;
};

export type ArticleDetail = ArticleSummary & {
  content: string;
};

export type StatisticItem = {
  id: string;
  key: string;
  label: string;
  displayValue: string;
  sortOrder: number;
};

export type TestimonialItem = {
  id: string;
  authorDisplayName: string;
  authorTitle: string | null;
  body: string;
  sortOrder: number;
};

export type FaqItem = {
  id: string;
  question: string;
  answer: string;
  sortOrder: number;
};

export type PublicSiteSettings = {
  brandName: string | null;
  brandNameEnglish: string | null;
  publicPhone: string | null;
  publicWhatsApp: string | null;
  publicEmail: string | null;
  socialLinks: string | null;
  footerText: string | null;
  consultationInfo?: string | null;
};

export type Paged<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};
