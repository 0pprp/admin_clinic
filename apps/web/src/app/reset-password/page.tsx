import { ResetPasswordForm } from "./ResetPasswordForm";

type SearchParams = Promise<{ email?: string; token?: string }>;

export default async function ResetPasswordPage({
  searchParams
}: {
  searchParams: SearchParams;
}) {
  const params = await searchParams;

  return <ResetPasswordForm email={params.email ?? ""} token={params.token ?? ""} />;
}
