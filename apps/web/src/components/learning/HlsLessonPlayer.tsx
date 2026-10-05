"use client";

import Hls from "hls.js";
import { useEffect, useId, useRef, useState } from "react";

const SPEED_OPTIONS = [0.5, 0.75, 1, 1.25, 1.5, 2] as const;
const AUTO_HIDE_MS = 3000;

type QualityOption = {
  index: number;
  label: string;
};

function isTrustedHlsUrl(url: string): boolean {
  try {
    if (url.startsWith("/api/media/v/")) {
      return true;
    }

    const parsed = new URL(url, window.location.origin);
    return parsed.origin === window.location.origin && parsed.pathname.startsWith("/api/media/v/");
  } catch {
    return false;
  }
}

function formatTime(seconds: number): string {
  if (!Number.isFinite(seconds) || seconds < 0) {
    return "0:00";
  }
  const total = Math.floor(seconds);
  const h = Math.floor(total / 3600);
  const m = Math.floor((total % 3600) / 60);
  const s = total % 60;
  if (h > 0) {
    return `${h}:${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
  }
  return `${m}:${String(s).padStart(2, "0")}`;
}

function levelLabel(height?: number, bitrate?: number): string {
  if (height) {
    return `${height}p`;
  }
  if (bitrate) {
    return `${Math.round(bitrate / 1000)}kbps`;
  }
  return "جودة";
}

type MenuKind = "quality" | "speed" | null;

export function HlsLessonPlayer({ src, title }: { src: string; title: string }) {
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const shellRef = useRef<HTMLDivElement | null>(null);
  const hlsRef = useRef<Hls | null>(null);
  const hideTimerRef = useRef<number | null>(null);
  const menuRef = useRef<MenuKind>(null);
  const hoveringControlsRef = useRef(false);
  const playingRef = useRef(false);
  const reactId = useId();

  const [ready, setReady] = useState(false);
  const [playing, setPlaying] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [buffered, setBuffered] = useState(0);
  const [speed, setSpeed] = useState(1);
  const [qualities, setQualities] = useState<QualityOption[]>([]);
  const [quality, setQuality] = useState<-1 | number>(-1);
  const [menu, setMenu] = useState<MenuKind>(null);
  const [controlsVisible, setControlsVisible] = useState(true);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [error, setError] = useState("");

  menuRef.current = menu;
  playingRef.current = playing;

  function clearHideTimer() {
    if (hideTimerRef.current) {
      window.clearTimeout(hideTimerRef.current);
      hideTimerRef.current = null;
    }
  }

  function scheduleHide() {
    clearHideTimer();
    // Keep bar only while a settings menu is open or pointer is on the control strip.
    if (menuRef.current || hoveringControlsRef.current) {
      setControlsVisible(true);
      return;
    }
    hideTimerRef.current = window.setTimeout(() => {
      if (menuRef.current || hoveringControlsRef.current) {
        return;
      }
      setMenu(null);
      setControlsVisible(false);
    }, AUTO_HIDE_MS);
  }

  function revealControls() {
    setControlsVisible(true);
    scheduleHide();
  }

  useEffect(() => {
    const video = videoRef.current;
    if (!video || !isTrustedHlsUrl(src)) {
      return;
    }

    setReady(false);
    setError("");
    setQualities([]);
    setQuality(-1);

    const absolute = src.startsWith("http") ? src : new URL(src, window.location.origin).toString();
    let hls: Hls | null = null;

    if (Hls.isSupported()) {
      hls = new Hls({
        enableWorker: true,
        lowLatencyMode: false,
        backBufferLength: 90,
        abrEwmaDefaultEstimate: 500_000
      });
      hlsRef.current = hls;
      hls.loadSource(absolute);
      hls.attachMedia(video);
      hls.on(Hls.Events.MANIFEST_PARSED, (_event, data) => {
        const next = data.levels
          .map((level, index) => ({
            index,
            label: levelLabel(level.height, level.bitrate),
            height: level.height || 0
          }))
          .sort((a, b) => a.height - b.height)
          .map(({ index, label }) => ({ index, label }));
        setQualities(next);
        setReady(true);
      });
      hls.on(Hls.Events.ERROR, (_event, data) => {
        if (data.fatal) {
          setError("تعذر تشغيل الفيديو. حدّث الصفحة وحاول مرة أخرى.");
        }
      });
    } else if (video.canPlayType("application/vnd.apple.mpegurl")) {
      video.src = absolute;
      const onLoaded = () => setReady(true);
      video.addEventListener("loadedmetadata", onLoaded);
      return () => {
        video.removeEventListener("loadedmetadata", onLoaded);
        video.removeAttribute("src");
        video.load();
      };
    } else {
      setError("المتصفح لا يدعم تشغيل هذا الفيديو.");
    }

    return () => {
      if (hls) {
        hls.destroy();
      }
      hlsRef.current = null;
    };
  }, [src]);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) {
      return;
    }

    const sync = () => {
      const isPlaying = !video.paused;
      setPlaying(isPlaying);
      playingRef.current = isPlaying;
      setCurrentTime(video.currentTime || 0);
      setDuration(video.duration || 0);
      if (video.buffered.length > 0) {
        setBuffered(video.buffered.end(video.buffered.length - 1));
      }
      // Always re-arm the 3s hide timer after play/pause changes.
      setControlsVisible(true);
      scheduleHide();
    };

    const events = ["play", "pause", "timeupdate", "loadedmetadata", "durationchange", "progress", "ended"] as const;
    events.forEach((event) => video.addEventListener(event, sync));
    sync();
    return () => {
      events.forEach((event) => video.removeEventListener(event, sync));
    };
  }, [src]);

  useEffect(() => {
    function onFullscreenChange() {
      const active = document.fullscreenElement === shellRef.current;
      setIsFullscreen(active);
      setControlsVisible(true);
      scheduleHide();
    }
    document.addEventListener("fullscreenchange", onFullscreenChange);
    return () => document.removeEventListener("fullscreenchange", onFullscreenChange);
  }, []);

  useEffect(() => {
    // Whenever a settings menu opens, keep the bar pinned.
    if (menu) {
      setControlsVisible(true);
      clearHideTimer();
    } else {
      scheduleHide();
    }
  }, [menu]);

  useEffect(() => {
    return () => clearHideTimer();
  }, []);

  function togglePlay() {
    const video = videoRef.current;
    if (!video) {
      return;
    }
    if (video.paused) {
      void video.play();
    } else {
      video.pause();
    }
    revealControls();
  }

  function seekTo(ratio: number) {
    const video = videoRef.current;
    if (!video || !Number.isFinite(video.duration) || video.duration <= 0) {
      return;
    }
    video.currentTime = Math.min(video.duration, Math.max(0, ratio * video.duration));
    setControlsVisible(true);
    scheduleHide();
  }

  function changeSpeed(next: number) {
    const video = videoRef.current;
    if (!video) {
      return;
    }
    video.playbackRate = next;
    setSpeed(next);
    setMenu(null);
    setControlsVisible(true);
  }

  function changeQuality(next: -1 | number) {
    const hls = hlsRef.current;
    setQuality(next);
    if (hls) {
      hls.currentLevel = next;
    }
    setMenu(null);
    setControlsVisible(true);
  }

  async function toggleFullscreen() {
    const shell = shellRef.current;
    if (!shell) {
      return;
    }
    try {
      if (document.fullscreenElement === shell) {
        await document.exitFullscreen();
      } else {
        await shell.requestFullscreen();
      }
    } catch {
      // ignore fullscreen denial
    }
    revealControls();
  }

  async function closeVideo() {
    const video = videoRef.current;
    if (video && !video.paused) {
      video.pause();
    }
    if (document.fullscreenElement) {
      try {
        await document.exitFullscreen();
      } catch {
        // ignore
      }
    }
    setMenu(null);
    setControlsVisible(false);
    clearHideTimer();
  }

  function openMenu(next: MenuKind) {
    setMenu((current) => (current === next ? null : next));
    setControlsVisible(true);
    clearHideTimer();
  }

  function onScreenTap() {
    // Tap/click the picture to show controls again and decide.
    setControlsVisible(true);
    scheduleHide();
  }

  if (!isTrustedHlsUrl(src)) {
    return (
      <div className="flex h-full items-center justify-center bg-surface px-6 text-center">
        <p className="max-w-md text-sm leading-8 text-muted">رابط التشغيل غير موثوق.</p>
      </div>
    );
  }

  const progress = duration > 0 ? (currentTime / duration) * 100 : 0;
  const bufferProgress = duration > 0 ? (buffered / duration) * 100 : 0;
  const qualityText =
    quality === -1 ? "تلقائي" : qualities.find((item) => item.index === quality)?.label || "تلقائي";
  const qualityMenuId = `${reactId}-quality`;
  const speedMenuId = `${reactId}-speed`;
  const showControls = controlsVisible || Boolean(menu);

  return (
    <div
      ref={shellRef}
      className="relative h-full w-full overflow-hidden bg-black text-white select-none"
      onKeyDown={(event) => {
        if (event.key === " " || event.key === "k") {
          event.preventDefault();
          togglePlay();
        }
        if (event.key === "f") {
          event.preventDefault();
          void toggleFullscreen();
        }
        if (event.key === "Escape") {
          if (menuRef.current) {
            setMenu(null);
            scheduleHide();
            return;
          }
          void closeVideo();
        }
      }}
      tabIndex={0}
      role="region"
      aria-label={title}
    >
      <video
        ref={videoRef}
        className="h-full w-full cursor-pointer bg-black object-contain"
        playsInline
        preload="metadata"
        title={title}
        controls={false}
        onClick={onScreenTap}
      />

      {error ? (
        <div className="absolute inset-0 flex items-center justify-center bg-black/70 px-6 text-center">
          <p className="max-w-md text-sm leading-8 text-white/90">{error}</p>
        </div>
      ) : null}

      {!ready && !error ? (
        <div className="pointer-events-none absolute inset-0 flex items-center justify-center text-sm text-white/70">
          جاري تجهيز الفيديو...
        </div>
      ) : null}

      <button
        type="button"
        className={`absolute top-3 end-3 z-30 inline-flex h-10 w-10 items-center justify-center rounded-sm bg-black/65 text-white ring-1 ring-white/25 transition hover:bg-accent ${
          showControls ? "opacity-100" : "pointer-events-none opacity-0"
        }`}
        aria-label="إغلاق الفيديو"
        title="إغلاق الفيديو"
        onClick={() => void closeVideo()}
      >
        <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" aria-hidden="true">
          <path
            d="M6 6l12 12M18 6L6 18"
            stroke="currentColor"
            strokeWidth="2.2"
            strokeLinecap="round"
          />
        </svg>
      </button>

      <div
        className={`absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/90 via-black/55 to-transparent px-3 pb-3 pt-12 transition duration-200 ${
          showControls ? "opacity-100" : "pointer-events-none opacity-0"
        }`}
        onMouseEnter={() => {
          hoveringControlsRef.current = true;
          setControlsVisible(true);
          clearHideTimer();
        }}
        onMouseLeave={() => {
          hoveringControlsRef.current = false;
          scheduleHide();
        }}
        onTouchStart={() => {
          hoveringControlsRef.current = true;
          setControlsVisible(true);
          clearHideTimer();
        }}
      >
        <div className="mb-3">
          <input
            type="range"
            min={0}
            max={1000}
            value={Math.round(progress * 10)}
            aria-label="شريط التقدم"
            className="player-seek h-1.5 w-full cursor-pointer appearance-none rounded-full bg-white/25"
            style={{
              background: `linear-gradient(to left, #f15a24 0%, #f15a24 ${progress}%, rgba(255,255,255,0.35) ${progress}%, rgba(255,255,255,0.35) ${bufferProgress}%, rgba(255,255,255,0.18) ${bufferProgress}%, rgba(255,255,255,0.18) 100%)`
            }}
            onChange={(event) => seekTo(Number(event.target.value) / 1000)}
          />
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            className="inline-flex h-9 w-9 items-center justify-center rounded-sm bg-white/10 hover:bg-white/20"
            aria-label={playing ? "إيقاف مؤقت" : "تشغيل"}
            onClick={togglePlay}
          >
            {playing ? (
              <svg viewBox="0 0 24 24" className="h-4 w-4 fill-current" aria-hidden="true">
                <path d="M6 5h4v14H6zm8 0h4v14h-4z" />
              </svg>
            ) : (
              <svg viewBox="0 0 24 24" className="h-4 w-4 fill-current" aria-hidden="true">
                <path d="M8 5v14l11-7z" />
              </svg>
            )}
          </button>

          <p className="min-w-[7rem] text-xs text-white/85" dir="ltr">
            {formatTime(currentTime)} / {formatTime(duration)}
          </p>

          <div className="ms-auto flex items-center gap-1.5">
            <div className="relative">
              <button
                type="button"
                className="inline-flex h-9 items-center gap-1 rounded-sm bg-white/10 px-2.5 text-xs hover:bg-white/20"
                aria-haspopup="menu"
                aria-expanded={menu === "speed"}
                aria-controls={speedMenuId}
                onClick={() => openMenu("speed")}
              >
                السرعة {speed === 1 ? "عادي" : `${speed}x`}
              </button>
              {menu === "speed" ? (
                <ul
                  id={speedMenuId}
                  role="menu"
                  className="absolute bottom-11 end-0 z-20 min-w-[7.5rem] overflow-hidden rounded-sm border border-white/15 bg-[#071b33] py-1 shadow-xl"
                >
                  {SPEED_OPTIONS.map((option) => (
                    <li key={option} role="none">
                      <button
                        type="button"
                        role="menuitemradio"
                        aria-checked={speed === option}
                        className={`block w-full px-3 py-2 text-right text-xs hover:bg-white/10 ${
                          speed === option ? "text-[#ff7a45]" : "text-white"
                        }`}
                        onClick={() => changeSpeed(option)}
                      >
                        {option === 1 ? "عادي (1x)" : `${option}x`}
                      </button>
                    </li>
                  ))}
                </ul>
              ) : null}
            </div>

            <div className="relative">
              <button
                type="button"
                className="inline-flex h-9 items-center gap-1 rounded-sm bg-white/10 px-2.5 text-xs hover:bg-white/20"
                aria-haspopup="menu"
                aria-expanded={menu === "quality"}
                aria-controls={qualityMenuId}
                onClick={() => openMenu("quality")}
              >
                الجودة {qualityText}
              </button>
              {menu === "quality" ? (
                <ul
                  id={qualityMenuId}
                  role="menu"
                  className="absolute bottom-11 end-0 z-20 min-w-[8rem] overflow-hidden rounded-sm border border-white/15 bg-[#071b33] py-1 shadow-xl"
                >
                  <li role="none">
                    <button
                      type="button"
                      role="menuitemradio"
                      aria-checked={quality === -1}
                      className={`block w-full px-3 py-2 text-right text-xs hover:bg-white/10 ${
                        quality === -1 ? "text-[#ff7a45]" : "text-white"
                      }`}
                      onClick={() => changeQuality(-1)}
                    >
                      تلقائي
                    </button>
                  </li>
                  {qualities.map((item) => (
                    <li key={`${item.label}-${item.index}`} role="none">
                      <button
                        type="button"
                        role="menuitemradio"
                        aria-checked={quality === item.index}
                        className={`block w-full px-3 py-2 text-right text-xs hover:bg-white/10 ${
                          quality === item.index ? "text-[#ff7a45]" : "text-white"
                        }`}
                        onClick={() => changeQuality(item.index)}
                      >
                        {item.label}
                      </button>
                    </li>
                  ))}
                </ul>
              ) : null}
            </div>

            <button
              type="button"
              className="inline-flex h-9 w-9 items-center justify-center rounded-sm bg-accent text-white hover:bg-accent-soft"
              aria-label={isFullscreen ? "خروج من ملء الشاشة" : "ملء الشاشة"}
              title={isFullscreen ? "خروج من ملء الشاشة" : "ملء الشاشة"}
              onClick={() => void toggleFullscreen()}
            >
              {isFullscreen ? (
                <svg viewBox="0 0 24 24" className="h-4 w-4 fill-current" aria-hidden="true">
                  <path d="M7 14H5v5h5v-2H7v-3zm0-4h2V7h3V5H5v5h2zm10 7h-3v2h5v-5h-2v3zm0-12h-3v2h3v3h2V5h-2z" />
                </svg>
              ) : (
                <svg viewBox="0 0 24 24" className="h-4 w-4 fill-current" aria-hidden="true">
                  <path d="M7 14H5v5h5v-2H7v-3zm12 5h-5v2h7v-7h-2v5zM7 5v3h2V7h3V5H7zm12 0h-5v2h3v3h2V5z" />
                </svg>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
