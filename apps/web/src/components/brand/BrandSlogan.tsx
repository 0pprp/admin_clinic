import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";

type SloganProps = {
  children: string;
  className?: string;
};

/** شعار الهوية بخط Amiri متصل؛ نفس عائلة تسميات اللون البرتقالي. */
export function BrandSlogan({ children, className = "" }: SloganProps) {
  return <BrandAccentLabel className={`font-bold ${className}`.trim()}>{children}</BrandAccentLabel>;
}
