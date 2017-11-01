const serverApiUrl: string = "http://localhost:64707/api";

const APP_CONFIG: ApplicationConfiguration = {
    usersUrl: `${serverApiUrl}/users`,
};

interface ApplicationConfiguration{
    usersUrl: string;
}

export default APP_CONFIG;
