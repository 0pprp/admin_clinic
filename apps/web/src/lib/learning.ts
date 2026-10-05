export type ContinueLearning = {
  courseId: string;
  courseSlug: string;
  courseTitle: string;
  lessonId: string;
  lessonTitle: string;
  lastWatchedAt: string | null;
};

export type DashboardSummary = {
  activeCoursesCount: number;
  completedLessonsCount: number;
  totalAccessibleLessonsCount: number;
  openPurchaseRequestsCount: number;
  lastLearningActivity: string | null;
  continueLearning: ContinueLearning | null;
};

export type StudentEnrollment = {
  id: string;
  courseId: string;
  courseSlug: string;
  courseTitle: string;
  thumbnailUrl: string | null;
  status: string;
  startedAt: string;
  expiresAt: string | null;
  accessType: string;
  canAccess: boolean;
  progressPercent: number;
  completedLessons: number;
  totalLessons: number;
  continueLessonId: string | null;
  continueLessonTitle: string | null;
};

export type StudentLessonSummary = {
  id: string;
  title: string;
  description: string | null;
  durationSeconds: number;
  sortOrder: number;
  isCompleted: boolean;
  watchedSeconds: number;
  lastWatchedAt: string | null;
  canAccess: boolean;
};

export type StudentSection = {
  id: string;
  title: string;
  sortOrder: number;
  lessons: StudentLessonSummary[];
};

export type StudentCourseLearning = {
  id: string;
  slug: string;
  title: string;
  description: string;
  thumbnailUrl: string | null;
  accessType: string;
  startedAt: string;
  expiresAt: string | null;
  progressPercent: number;
  completedLessons: number;
  totalLessons: number;
  courseCompleted: boolean;
  continueLesson: ContinueLearning | null;
  sections: StudentSection[];
};

export type LessonNeighbor = {
  id: string;
  title: string;
};

export type StudentLessonLearning = {
  id: string;
  title: string;
  description: string | null;
  durationSeconds: number;
  courseId: string;
  courseSlug: string;
  courseTitle: string;
  progressPercent: number;
  isCompleted: boolean;
  watchedSeconds: number;
  lastWatchedAt: string | null;
  previousLesson: LessonNeighbor | null;
  nextLesson: LessonNeighbor | null;
  sections: StudentSection[];
};

export type LessonProgress = {
  lessonId: string;
  watchedSeconds: number;
  isCompleted: boolean;
  lastWatchedAt: string | null;
  completedAt: string | null;
};

export type LessonPlayback = {
  lessonId: string;
  playbackUnavailable: boolean;
  message: string;
  playbackUrl?: string | null;
  expiresAt?: string | null;
  provider?: string | null;
  kind?: string | null;
};

export function enrollmentStatusLabel(status: string, canAccess: boolean, expiresAt: string | null): string {
  if (status === "Active" && !canAccess && expiresAt) {
    return "منتهية";
  }

  switch (status) {
    case "Active":
      return "فعالة";
    case "Suspended":
      return "معلقة";
    case "Revoked":
      return "مسحوبة";
    case "Expired":
      return "منتهية";
    default:
      return status;
  }
}

export function enrollmentRestrictionMessage(status: string, canAccess: boolean, expiresAt: string | null): string | null {
  if (canAccess) {
    return null;
  }

  if (status === "Suspended") {
    return "الوصول إلى هذه الدورة معلّق.";
  }

  if (status === "Expired" || (status === "Active" && expiresAt)) {
    return "انتهت مدة الوصول إلى هذه الدورة.";
  }

  return "الوصول إلى هذه الدورة غير متاح حالياً.";
}

export function shortDisplayName(fullName: string): string {
  const first = fullName.trim().split(/\s+/)[0];
  return first || fullName;
}

export function lessonHref(courseSlug: string, lessonId: string): string {
  return `/dashboard/courses/${courseSlug}/lessons/${lessonId}`;
}
