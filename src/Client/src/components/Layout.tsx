import { ActionIcon, Container, Group, Image, Text, Tooltip } from "@mantine/core";
import { NavLink, Outlet, useNavigate } from "react-router";
import logo from "@/assets/images/logo.png";
import { useAuth } from "@/hooks/useAuth";
import { Roles } from "@/types/auth.types";
import classes from "./Layout.module.css";

// Layout-роут с <Outlet/>: шапка описана один раз, а не копируется по страницам.
export default function Layout() {
    const { user, signOut } = useAuth();
    const navigate = useNavigate();

    function handleSignOut() {
        signOut();
        void navigate("/login", { replace: true });
    }

    return (
        <>
            <header className={classes.header}>
                <Container size="xl">
                    <Group justify="space-between" h={56}>
                        <Group gap="lg">
                            <NavLink to="/orders">
                                <Image src={logo} alt="CRNC OMS" h={28} w="auto" />
                            </NavLink>
                            {user?.role === Roles.Admin && (
                                <NavLink to="/users" className={classes.link} data-testid="nav-users">
                                    Users
                                </NavLink>
                            )}
                            <NavLink to="/orders" className={classes.link} data-testid="nav-orders">
                                Orders
                            </NavLink>
                            <NavLink to="/jobs" className={classes.link} data-testid="nav-jobs">
                                Jobs
                            </NavLink>
                        </Group>
                        <Group gap="xs">
                            <Text size="sm" data-testid="user-login">{user?.login}</Text>
                            <Tooltip label="Sign out">
                                <ActionIcon
                                    variant="subtle"
                                    aria-label="Sign out"
                                    onClick={handleSignOut}
                                    data-testid="user-signout"
                                >
                                    ⎋
                                </ActionIcon>
                            </Tooltip>
                        </Group>
                    </Group>
                </Container>
            </header>
            <Container size="xl" py="md" component="main">
                <Outlet />
            </Container>
        </>
    );
}
