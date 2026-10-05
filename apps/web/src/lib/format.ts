export function formatIqd(amount: number): string {
  return `${new Intl.NumberFormat("ar-IQ").format(amount)} د.ع`;
}

export function formatDate(value: string | null): string | null {
  if (!value) {
    return null;
  }

  return new Intl.DateTimeFormat("ar-IQ", {
    year: "numeric",
    month: "long",
    day: "numeric"
  }).format(new Date(value));
}

export function formatBaghdadDateTime(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }

  return new Intl.DateTimeFormat("ar-IQ", {
    timeZone: "Asia/Baghdad",
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit"
  }).format(new Date(value));
}

export function formatDuration(totalSeconds: number): string {
  const seconds = Math.max(0, Math.floor(totalSeconds));
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);

  if (hours === 0) {
    if (minutes <= 0) {
      return seconds > 0 ? "أقل من دقيقة" : "0 دقيقة";
    }

    return minutes === 1 ? "1 دقيقة" : `${minutes} دقيقة`;
  }

  const hourLabel = hours === 1 ? "1 ساعة" : `${hours} ساعات`;
  if (minutes === 0) {
    return hourLabel;
  }

  const minuteLabel = minutes === 1 ? "1 دقيقة" : `${minutes} دقيقة`;
  return `${hourLabel} و${minuteLabel}`;
}

export function courseAccessLabel(accessType: string, durationDays: number | null): string {
  if (accessType === "LimitedDuration" && durationDays && durationDays > 0) {
    return `وصول لمدة ${durationDays} يوماً بعد التفعيل`;
  }

  return "وصول مدى الحياة";
}

export function courseLevelLabel(level: string): string {
  switch (level) {
    case "Beginner":
      return "مبتدئ";
    case "Intermediate":
      return "متوسط";
    case "Advanced":
      return "متقدم";
    default:
      return level;
  }
}
