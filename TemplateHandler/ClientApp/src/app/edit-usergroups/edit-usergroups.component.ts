import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Location } from '@angular/common';
import { User, UserGroup, getRightName, UserGroupMember } from './../services/interfaces';
import { Observable } from 'rxjs';
import { UserGroupService } from '../services/usergroupservice';
import { UserService } from '../services/userservice';


@Component({
  selector: 'app-edit-usergroups',
  templateUrl: './edit-usergroups.component.html',
  styleUrls: ['./edit-usergroups.component.css']
})
export class EditUserGroupsComponent implements OnInit {

	user: User;
	operation:String;
	addUser=false;
	groupData: Observable<UserGroup>;
	groupId: string;
	unique=true;
	userGroupService= new  UserGroupService(this.http);
	userService= new  UserService(this.http);
	users: Observable<User[]>;
	members: Observable<UserGroupMember[]>;
	getRightName=getRightName;

		
	constructor(private http:HttpClient, private activatedRoute: ActivatedRoute, private location:Location) { 
	}

	ngOnInit() {
		this.operation=this.activatedRoute.snapshot.paramMap.get('operation');
		this.user=JSON.parse(localStorage.getItem('user'));
		if(this.operation=="edit"){
			this.groupId=this.activatedRoute.snapshot.paramMap.get('id');
			this.getGroup(this.groupId);
		}
	}

	back(){
		this.location.back();
	}

	getGroup(id){
		this.groupData= this.userGroupService.getUserGroup(id);
        this.groupData.subscribe((response)=> {}, err => {
            console.log(err);
		});
		this.members= this.userGroupService.getUserGroupMembers(id);
		this.members.subscribe((response)=> {}, err => {
            console.log(err);
		});
	}

	editGroup(form:NgForm){
		let data=form.value;
		this.groupData.subscribe((response:UserGroup)=>{
			if(data.groupName!=response.name){
				this.userGroupService.nameTeszt(data.groupName).subscribe((isUnique:boolean)=>{
					if(isUnique){
						this.unique=true;
						this.updateGroup(data);
					}else{
						this.unique=false;
					}
				},err=>{
					console.log(err);
				});
			}else{
				this.unique=true;
				this.updateGroup(data);
			}
		},err=>{
			console.log(err);
		});
	}

	updateGroup(data){
		this.groupData.subscribe((response : UserGroup)=>{
			let updated = null;
			if(Object.keys(data).length == 2){
				updated = this.userGroupService.editGroup("group",response.id,data.description,data.groupName);
			}else{
				updated = this.userGroupService.editGroup("all",response.id,data.description,data.groupName,data.userName,data.rights);
			}
			if(updated != null){
				updated.subscribe(response=> {
					this.getGroup(this.groupId);
				}, err => {
					console.log(err);
				});
			}
		},err => {
			console.log(err);
		});
	}

	createGroup(form:NgForm){
		let data={} as UserGroup;
		data.name=form.value.groupName;
		data.description=form.value.description;
		this.userGroupService.nameTeszt(data.name).subscribe((isUnique:boolean)=>{	
			if(isUnique){
				this.unique=true;
				let create = this.userGroupService.createGroup(data);
				create.subscribe((result) =>{
					this.back();
				},err =>{
					console.log(err);
				});
			}else{
				this.unique=false;
			}
		},err=>{
			console.log(err);
		});
	}

	removeUser(id){
		let removed = this.userGroupService.removeUser(id);
		removed.subscribe((response)=> {
			let members = this.userGroupService.getUserGroupMembers(this.groupId);
			members.subscribe((response)=> {}, err => {
				console.log(err);
			});
		}, err => {
			console.log(err);
		});
	}

	changeRight(id, right){
		console.log("id: "+id+", right: "+right);
		/*let removed = this.userGroupService.removeUser(id);
		removed.subscribe((response)=> {
			let members = this.userGroupService.getUserGroupMembers(this.groupId);
			members.subscribe((response)=> {}, err => {
				console.log(err);
			});
		}, err => {
			console.log(err);
		});*/
	}

	filtering(filter){
		if(filter == ""){
			this.users = this.userService.getAllUser();
			this.users.subscribe((response)=> {}, err => {
				console.log(err);
			});
		}else{
			this.users = this.userService.getFilteredUsers(filter);
			this.users.subscribe((response)=> {}, err => {
				console.log(err);
			});
		}
	}

	wantAdd(){
		this.addUser=!this.addUser;
		if(this.addUser){
			this.users = this.userService.getAllUser();
			this.users.subscribe((response)=> {}, err => {
				console.log(err);
			});
		}
	}

}