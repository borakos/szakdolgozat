import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {User, Role, getRoleName} from './../services/interfaces';
import { Observable } from 'rxjs';
import { UserService } from '../services/userservice';


@Component({
    selector: 'app-users',
    templateUrl: './users.component.html',
    styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {

	getRoleName=getRoleName;
	users:Observable<User[]>;
	userService= new UserService(this.http);

    constructor(private http: HttpClient) { 
	}

    ngOnInit() {
		this.getAllUser();
	}

	getAllUser(){
		this.users=this.userService.getAllUser();
		this.users.subscribe((response)=>{}, err => {
			console.log(err);
		});
	}

	deleteUser(id){
		let deleted=this.userService.deleteUser(id);
		deleted.subscribe((response)=>{
			this.getAllUser();
		}, err => {
			console.log(err);
		});
	}
}
