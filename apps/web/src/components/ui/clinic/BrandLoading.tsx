import { BrandMark } from "./BrandMark";
import { brand } from "@/lib/content/brand";

export function BrandLoading({ label = "جاري التحميل..." }: { label?: string }) {
  return (
    <div className="flex min-h-[50vh] flex-col items-center justify-center gap-5 bg-background px-6 text-foreground">
      <BrandMark className="h-14 w-14 animate-pulse" title={brand.nameAr} />
      <p className="text-sm text-muted">{label}</p>
    </div>
  );
}
