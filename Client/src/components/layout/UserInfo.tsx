import * as React from 'react';
import CurrentUserContext from '../../auth/CurrentUserContext';
import { Segment, Icon, Header } from 'semantic-ui-react';

export default class UserInfo extends React.Component<any, any> {
  
  
    public render() {
        return (
            <Header icon={{link: true, name: "sign out", title: "logout"}} content={`${CurrentUserContext.login}`} size="small"/>

        );
  }
}
