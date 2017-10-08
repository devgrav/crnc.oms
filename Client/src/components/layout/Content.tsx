import * as React from "react";
import {Header} from "semantic-ui-react";

const Content: React.StatelessComponent = (props: ContentProps) => {
    return (
        props.children
    );
};

export default Content;

interface ContentProps{
    children?: React.ReactElement<any>;
}
