import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {UserGroup} from './../services/interfaces';
import { Observable } from 'rxjs';
import { UserGroupService } from '../services/usergroupservice';

@Component({
  selector: 'app-usergroups',
  templateUrl: './usergroups.component.html',
  styleUrls: ['./usergroups.component.css']
})
export class UserGroupsComponent implements OnInit {

  	userGroups:Observable<UserGroup[]>;
	userGroupService= new UserGroupService(this.http);
	error:String;
	errorOccured = false;

    constructor(private http: HttpClient) { 
	}

    ngOnInit() {
		this.getAllUserGroup();
	}

	getAllUserGroup(){
		this.userGroups=this.userGroupService.getAllUserGroup();
		this.userGroups.subscribe((response)=>{}, err => {
			this.error = err.error;
			this.errorOccured = true;
			console.log(err);
		});
	}

	deleteUserGroup(id){
		let deleted=this.userGroupService.deleteUserGroup(id);
		deleted.subscribe((response)=>{
			this.getAllUserGroup();
		}, err => {
			this.error = err.error;
			this.errorOccured = true;
			console.log(err);
		});
	}
}
