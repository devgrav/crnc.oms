const securityApiUrl: string = "http://localhost:64707/api";
const salesApiUrl: string = "http://localhost:64708/api";

const APP_CONFIG: ApplicationConfiguration = {
    usersUrl: `${securityApiUrl}/users`,
    rolesUrl: `${securityApiUrl}/roles`,
    accountsUrl: `${securityApiUrl}/accounts`,
    ordersUrl: `${salesApiUrl}/orders`
};

interface ApplicationConfiguration{
    usersUrl: string;
    rolesUrl: string;
    accountsUrl: string;
    ordersUrl: string;
}

export default APP_CONFIG;
