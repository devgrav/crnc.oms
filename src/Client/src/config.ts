if(!process.env.REACT_APP_SECURITY_API_URL || !process.env.REACT_APP_SALES_API_URL)
    throw Error("Not found api urls in configuration of enviroment variabels")

console.log("REACT_APP_SECURITY_API_URL: "+process.env.REACT_APP_SECURITY_API_URL)
console.log("REACT_APP_SALES_API_URL: "+process.env.REACT_APP_SECURITY_API_URL)
console.log("NODE_ENV: "+process.env.NODE_ENV)

const securityApiUrl: string = process.env.REACT_APP_SECURITY_API_URL;
const salesApiUrl: string = process.env.REACT_APP_SALES_API_URL; 

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
