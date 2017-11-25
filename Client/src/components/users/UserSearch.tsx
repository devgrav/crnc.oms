import * as React from "react";
import { Form, Segment, Header, Icon, Button } from "semantic-ui-react";

export default class UserSearch
    extends React.Component<any>{

    constructor(props: any){
        super(props);
    }

    public render(){
        return (
            <Segment basic>
                <Header attached="top">
                    <Icon name="search"/>
                    <Header.Content>
                        Search
                    </Header.Content>
                </Header>
                <Segment basic attached clearing>
                    <Form>
                        <Form.Group>
                            <Form.Input label="Full name" width={4}/>
                            <Form.Input label="Login" width={2}/>
                            <Form.Select label="Role" width={4}/>
                        </Form.Group>
                        <Form.Checkbox label="Active"/>
                        <Form.Button icon="cancel" content="Clear" floated="right" primary/>
                    </Form>
                </Segment>
            </Segment>
        );
    }
}
