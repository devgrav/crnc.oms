import { MantineProvider } from "@mantine/core";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router";
import { describe, expect, it } from "vitest";
import ProtectedRoute from "../ProtectedRoute";
import { AuthContext, type AuthContextValue } from "@/auth/AuthContext";
import { makeCurrentUser } from "@/test/factories";
import { Roles, type UserRole } from "@/types/auth.types";

function renderGuard(role: UserRole | null, allowed: UserRole[]) {
    const value: AuthContextValue = {
        user: role ? makeCurrentUser({ role }) : null,
        isAuthenticated: role !== null,
        signIn: () => Promise.resolve({ success: true }),
        signOut: () => undefined,
    };

    return render(
        <MantineProvider>
        <AuthContext value={value}>
            <MemoryRouter initialEntries={["/protected"]}>
                <Routes>
                    <Route path="/login" element={<div>login page</div>} />
                    <Route element={<ProtectedRoute roles={allowed} />}>
                        <Route path="/protected" element={<div>protected page</div>} />
                    </Route>
                </Routes>
            </MemoryRouter>
        </AuthContext>
        </MantineProvider>,
    );
}

describe("ProtectedRoute", () => {
    it("AllowedRole_MatchingRoute_RendersPage", () => {
        //Arrange, Act
        renderGuard(Roles.Manager, [Roles.Manager, Roles.MainManager]);

        //Assert
        expect(screen.getByText("protected page")).toBeInTheDocument();
    });

    it("WrongRole_DeclaredRoutes_ShowsForbidden", () => {
        //Arrange, Act
        renderGuard(Roles.Manager, [Roles.Admin]);

        //Assert - именно Forbidden, а не редирект и не пустой экран
        expect(screen.getByTestId("forbidden")).toBeInTheDocument();
    });

    it("AdminRole_RouteWithoutAdmin_StillRendersPage", () => {
        //Arrange - намеренное поведение, перенесённое из старого PrivateRoute
        //Act
        renderGuard(Roles.Admin, [Roles.Manager]);

        //Assert
        expect(screen.getByText("protected page")).toBeInTheDocument();
    });

    it("NotAuthenticated_AnyRoute_RedirectsToLogin", () => {
        //Arrange, Act
        renderGuard(null, [Roles.Manager]);

        //Assert
        expect(screen.getByText("login page")).toBeInTheDocument();
    });
});
