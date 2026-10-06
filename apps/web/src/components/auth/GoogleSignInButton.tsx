"use client";

import Script from "next/script";
import { useCallback, useEffect, useId, useRef, useState } from "react";

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: { credential: string }) => void;
            auto_select?: boolean;
            ux_mode?: "popup" | "redirect";
          }) => void;
          renderButton: (
            parent: HTMLElement,
            options: {
              type?: string;
              theme?: string;
              size?: string;
              text?: string;
              shape?: string;
              width?: number;
              locale?: string;
            }
          ) => void;
        };
      };
    };
  }
}

type Props = {
  onCredential: (idToken: string) => void | Promise<void>;
  disabled?: boolean;
  label?: string;
};

export function GoogleSignInButton({ onCredential, disabled, label = "المتابعة عبر Google" }: Props) {
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID?.trim() ?? "";
  const hostId = useId().replace(/:/g, "");
  const hostRef = useRef<HTMLDivElement>(null);
  const [ready, setReady] = useState(false);
  const [scriptError, setScriptError] = useState(false);
  const callbackRef = useRef(onCredential);
  callbackRef.current = onCredential;

  const mountButton = useCallback(() => {
    if (!clientId || !hostRef.current || !window.google?.accounts?.id) {
      return;
    }

    hostRef.current.innerHTML = "";
    window.google.accounts.id.initialize({
      client_id: clientId,
      callback: (response) => {
        if (response.credential) {
          void callbackRef.current(response.credential);
        }
      },
      auto_select: false,
      ux_mode: "popup"
    });

    window.google.accounts.id.renderButton(hostRef.current, {
      type: "standard",
      theme: "outline",
      size: "large",
      text: "continue_with",
      shape: "rectangular",
      width: 320,
      locale: "ar"
    });
    setReady(true);
  }, [clientId]);

  useEffect(() => {
    if (clientId && window.google?.accounts?.id) {
      mountButton();
    }
  }, [clientId, mountButton]);

  if (!clientId) {
    return (
      <p className="rounded-xl border border-dashed border-border bg-background px-3 py-3 text-center text-xs text-muted">
        سجّل `NEXT_PUBLIC_GOOGLE_CLIENT_ID` لتفعيل زر Google.
      </p>
    );
  }

  return (
    <div className="space-y-2">
      <Script
        src="https://accounts.google.com/gsi/client"
        strategy="afterInteractive"
        onLoad={mountButton}
        onError={() => setScriptError(true)}
      />
      <div
        id={`google-btn-${hostId}`}
        ref={hostRef}
        className={`flex min-h-11 items-center justify-center ${disabled ? "pointer-events-none opacity-50" : ""}`}
        aria-label={label}
      />
      {!ready && !scriptError ? <p className="text-center text-xs text-muted">جاري تحميل Google...</p> : null}
      {scriptError ? <p className="text-center text-xs text-red-700">تعذر تحميل Google Sign-In.</p> : null}
    </div>
  );
}
