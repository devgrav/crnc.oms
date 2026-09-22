import { Component, type ErrorInfo, type ReactNode } from "react";

interface ErrorBoundaryProps {
    children: ReactNode;
}

interface ErrorBoundaryState {
    hasError: boolean;
}

// Единственный классовый компонент в приложении: хуковой замены componentDidCatch
// в React 19 нет. Без него упавший компонент даёт белый экран - §7.1 плана.
export default class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
    public state: ErrorBoundaryState = { hasError: false };

    public static getDerivedStateFromError(): ErrorBoundaryState {
        return { hasError: true };
    }

    public componentDidCatch(error: Error, info: ErrorInfo): void {
        // Логирование пойдёт в отдельный слой, когда он появится; пока ошибка
        // не должна исчезать бесследно.
        void error;
        void info;
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
