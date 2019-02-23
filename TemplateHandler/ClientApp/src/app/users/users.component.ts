import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {User, Role} from './../services/interfaces';


@Component({
    selector: 'app-users',
    templateUrl: './users.component.html',
    styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {

    public users: User[];

    constructor(private http: HttpClient) { 
	}

    ngOnInit() {
		this.getAllUser();
	}
	
	getAllUser(){
		let token = localStorage.getItem("jwt");
        this.http.get("https://localhost:44396/api/users/index", {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: User[] )=> {
			this.users=response;
			for(let i=0;i<response.length;i++){
				this.users[i].role=Role[response[i].role];
			}
        }, err => {
            console.log(err);
		});
	}

	deleteUser(id){
		let token = localStorage.getItem("jwt");
        this.http.delete("https://localhost:44396/api/users/delete/"+id, {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: User[] )=> {
			this.getAllUser();
        }, err => {
            console.log(err);
		});
	}
}
