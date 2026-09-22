declare const __APP_VERSION__: string;
declare const __APP_COMMIT__: string;

export const appVersion = __APP_VERSION__;
export const appCommit = __APP_COMMIT__;

export function formatAppVersion(version = appVersion, commit = appCommit): string {
    return commit === "dev" ? `v${version}` : `v${version} · ${commit}`;
}
