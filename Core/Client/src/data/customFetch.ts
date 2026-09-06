import { useUserStore } from '@/store/userStore';
import type { TokenResponse } from '@/types/tokenResponse';

let refreshPromise: Promise<boolean> | null = null;

const refreshAccessToken = (): Promise<boolean> => {
  const refreshToken = localStorage.getItem('refreshToken');
  if (!refreshToken) {
    return Promise.resolve(false);
  }

  refreshPromise ??= (async () => {
    try {
      const res = await fetch('/api/auth/refresh', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      });

      if (!res.ok) {
        return false;
      }

      const tokens = (await res.json()) as TokenResponse;
      sessionStorage.setItem('accessToken', tokens.accessToken);
      localStorage.setItem('refreshToken', tokens.refreshToken);

      const store = useUserStore();
      store.setAccessToken(tokens.accessToken);
      store.setRefreshToken(tokens.refreshToken);

      return true;
    } catch {
      return false;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
};

const toResult = async <T>(res: Response): Promise<T> => {
  const body = [204, 205, 304].includes(res.status) ? null : await res.text();
  const data = body ? JSON.parse(body) : undefined;
  return { data, status: res.status, headers: res.headers } as T;
};

const withAuthHeader = (options: RequestInit): RequestInit => ({
  ...options,
  headers: { ...options.headers, Authorization: `Bearer ${sessionStorage.getItem('accessToken')}` },
});

export const customFetch = async <T>(url: string, options: RequestInit): Promise<T> => {
  const res = await fetch(url, options);

  if (res.status !== 401) {
    return toResult<T>(res);
  }

  if (!(await refreshAccessToken())) {
    return toResult<T>(res);
  }

  const retryRes = await fetch(url, withAuthHeader(options));
  return toResult<T>(retryRes);
};
