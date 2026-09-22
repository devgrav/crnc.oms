// Значения подставляются на сборке через define в vite.config.ts.
declare const __APP_VERSION__: string;
declare const __APP_COMMIT__: string;

export const appVersion = __APP_VERSION__;
export const appCommit = __APP_COMMIT__;

// То, что видит пользователь: "v1.0.0" для локальной сборки и "v1.0.0 · a1b2c3d"
// для сборки из CI, где коммит известен.
export function formatAppVersion(version = appVersion, commit = appCommit): string {
    return commit === "dev" ? `v${version}` : `v${version} · ${commit}`;
}
