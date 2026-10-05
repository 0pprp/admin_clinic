import Image from "next/image";

export function CourseThumbnail({
  title,
  src,
  sizes = "(max-width: 768px) 100vw, 320px"
}: {
  title: string;
  src: string | null;
  sizes?: string;
}) {
  return (
    <div className="relative aspect-[16/10] overflow-hidden bg-surface-warm">
      {src ? (
        <Image
          src={src}
          alt={`غلاف دورة ${title}`}
          fill
          className="object-cover"
          sizes={sizes}
          unoptimized={!src.startsWith("/")}
        />
      ) : (
        <div className="flex h-full items-end p-5">
          <span className="text-xs tracking-[0.2em] text-accent">دورة</span>
        </div>
      )}
    </div>
  );
}
