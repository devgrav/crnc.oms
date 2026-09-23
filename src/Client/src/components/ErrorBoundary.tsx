import { Component, type ReactNode } from "react";

interface ErrorBoundaryProps {
    children: ReactNode;
}

interface ErrorBoundaryState {
    hasError: boolean;
}

export default class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
    public state: ErrorBoundaryState = { hasError: false };

    public static getDerivedStateFromError(): ErrorBoundaryState {
        return { hasError: true };
    }

    public render(): ReactNode {
        if (this.state.hasError) {
            return (
                <div role="alert" style={{ padding: "2rem", fontFamily: "sans-serif" }}>
                    <h1>Something went wrong</h1>
                    <p>Reload the page. If it keeps happening, sign in again.</p>
                </div>
            );
        }

        return this.props.children;
    }
}
