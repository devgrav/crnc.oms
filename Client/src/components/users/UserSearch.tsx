import * as React from "react";
import { Button, Form, Icon, Modal, Segment } from "semantic-ui-react";

export default class UserSearch
    extends React.Component<any>{

    constructor(props: any){
        super(props);
    }

    public render(){
        return (
            <Modal open={true} closeIcon>
                <Modal.Header><Icon name="search"/>Search</Modal.Header>
                <Modal.Content as={Segment} basic clearing>
                <Form id="searchForm">
                    <Form.Group inline>
                        <Form.Input label="Full name"/>
                        <Form.Input label="Login" />
                        <Form.Select label="Role"/>
                    </Form.Group>
                    <Form.Checkbox label="Active"/>
                </Form>
                </Modal.Content>
                <Modal.Actions>
                    <Button basic color="green" type="submit" content="Search" form="searchForm"/>
                    <Button basic color="red" type="reset" content="Cancel"/>
                </Modal.Actions>
        </Modal>
        );
    }
}
