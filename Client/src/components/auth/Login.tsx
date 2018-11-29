import * as React from 'react';
import { Form, Button } from 'semantic-ui-react';

export default class Login extends React.Component<any, any> {
  
    public render() {
        return (    
        <Form>
            <Form.Input label="Login"/>
            <Form.Input label="Password" type="password"/>
            <Button type="submit"/>
        </Form>
        );
  }
}
