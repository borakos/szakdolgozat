import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NgForm } from '@angular/forms';

enum Role{
	admin=0,
	user,
}

interface User {
    id: number;
    userName: string;
    nativeName: string;
    email: string;
    role: string;
}

@Component({
    selector: 'app-users',
    templateUrl: './users.component.html',
    styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {

    public users: User[];

    constructor(private http: HttpClient) { 
		console.log("Constructor");
	}

    ngOnInit() {
		this.getAllUser();
		console.log("ngOnInit");
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

	createUser(form: NgForm) {
		let credentials = JSON.stringify(form.value);
		let token = localStorage.getItem("jwt");
		console.log(credentials);
		this.http.post("https://localhost:44396/api/users/create", credentials, {
			headers: new HttpHeaders({
				"Authorization": "Bearer " + token,
				"Content-Type": "application/json"
			})
		}).subscribe(response => {
			this.getAllUser();
		}, err => {
			console.log(err);
		});
	}
}
