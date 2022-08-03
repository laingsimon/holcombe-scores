import React, {Component} from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Access} from '../../api/access';
import {Alert} from '../Alert';

/*
* Props:
* - access
* - updateAccess()
*
* Events:
* - onAccessDeleted(userId)
* - onAccessChanged(access)
* */
// noinspection JSUnresolvedVariable
export class EditAccess extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: false,
            proposed: Object.assign({}, this.props.access)
        };
        this.updateAccess = this.updateAccess.bind(this);
        this.accessChanged = this.accessChanged.bind(this);
        this.removeAccess = this.removeAccess.bind(this);
        let http = new Http(new Settings());
        this.accessApi = new Access(http);
    }

    //event handlers
    async removeAccess() {
        if (!window.confirm('Are you sure you want to remove your access')) {
            return;
        }

        this.setState({loading: true});

        const result = await this.accessApi.deleteAccess(this.props.access.userId);

        if (result.success) {
            this.setState({mode: 'access'});
            if (this.props.onAccessDeleted) {
                await this.props.onAccessDeleted(this.props.access.userId);
            }
        } else {
            alert('Could not delete your details');
        }
    }

    async updateAccess() {
        if (!this.state.proposed.name) {
            alert('You need to enter a name');
            return;
        }

        const currentAccessCopy = Object.assign({}, this.props.access);
        const accessUpdate = Object.assign(currentAccessCopy, this.state.proposed);

        this.setState({loading: true});

        const result = await this.accessApi.updateAccess(accessUpdate.teamId, accessUpdate.userId, accessUpdate.name, accessUpdate.admin, accessUpdate.manager);

        if (result.success) {
            if (this.props.onAccessChanged) {
                await this.props.onAccessChanged(accessUpdate);
            }
        } else {
            alert('Could not update your access');
        }
    }

    accessChanged(event) {
        const name = event.target.getAttribute('name');
        const value = event.target.value;
        const proposed = Object.assign({}, this.state.proposed);
        proposed[name] = value;

        this.setState({
            proposed: proposed
        });
    }

    render() {
        try {
            return (<div>
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">Your name</span>
                    </div>
                    <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name"
                           value={this.state.proposed.name} onChange={this.accessChanged}/>
                </div>
                <button type="button" className="btn btn-primary margin-right" onClick={this.updateAccess}>Update details</button>
                <button type="button" className="btn btn-danger" onClick={this.removeAccess}>Remove access</button>
            </div>)
        } catch (e) {
            console.error(e);
            return (<Alert errors={[`Error rendering component: ${e.message}`]}/>);
        }
    }
}
