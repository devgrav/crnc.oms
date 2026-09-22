import { useState, type FormEvent } from "react";
import { Alert, Button, Card, Center, Image, PasswordInput, Stack, TextInput } from "@mantine/core";
import { useLocation, useNavigate } from "react-router";
import logo from "@/assets/images/logo.png";
import { useAuth } from "@/hooks/useAuth";
import { useFormValidation } from "@/hooks/useFormValidation";

interface RedirectState {
    from?: string;
}

export default function LoginPage() {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const validation = useFormValidation();
    const { signIn } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    const from = (location.state as RedirectState | null)?.from ?? "/orders";

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setIsLoading(true);
        validation.clearAllErrors();

        const result = await signIn(login, password);

        setIsLoading(false);

        if (result.success) {
            void navigate(from, { replace: true });
            return;
        }

        validation.setFromResult(result);
    }

    return (
        <Center h="100vh">
            <Card withBorder shadow="sm" padding="lg" w={360}>
                <Card.Section inheritPadding py="md">
                    <Center>
                        <Image src={logo} alt="CRNC OMS" w={72} />
                    </Center>
                </Card.Section>
                <form onSubmit={(event) => void handleSubmit(event)}>
                    <Stack gap="sm">
                        {validation.generalError && (
                            <Alert color="red" data-testid="login-error">
                                {validation.generalError}
                            </Alert>
                        )}
                        <TextInput
                            label="Login"
                            value={login}
                            onChange={(event) => {
                                setLogin(event.currentTarget.value);
                                validation.clearAllErrors();
                            }}
                            disabled={isLoading}
                            data-testid="login-login"
                        />
                        <PasswordInput
                            label="Password"
                            value={password}
                            onChange={(event) => {
                                setPassword(event.currentTarget.value);
                                validation.clearAllErrors();
                            }}
                            disabled={isLoading}
                            data-testid="login-password"
                        />
                        <Button
                            type="submit"
                            loading={isLoading}
                            disabled={!login || !password}
                            data-testid="login-submit"
                        >
                            Sign In
                        </Button>
                    </Stack>
                </form>
            </Card>
        </Center>
    );
}
