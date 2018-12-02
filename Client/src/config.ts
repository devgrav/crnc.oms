const serverApiUrl: string = "http://localhost:64707/api";

const APP_CONFIG: ApplicationConfiguration = {
    usersUrl: `${serverApiUrl}/users`,
    rolesUrl: `${serverApiUrl}/roles`,
    accountsUrl: `${serverApiUrl}/accounts`
};

interface ApplicationConfiguration{
    usersUrl: string;
    rolesUrl: string;
    accountsUrl: string;
}

export default APP_CONFIG;
