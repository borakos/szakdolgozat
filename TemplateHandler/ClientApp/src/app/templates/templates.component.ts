import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {User, GrouppedTemplates} from './../services/interfaces';

@Component({
	selector: 'app-templates',
	templateUrl: './templates.component.html',
	styleUrls: ['./templates.component.css']
})
export class TemplatesComponent implements OnInit {

	groups: GrouppedTemplates[];
	user: User;

	constructor(private http:HttpClient) { }

	ngOnInit() {
		this.user=JSON.parse(localStorage.getItem('user'));
		this.getGrouppedTemplates(this.user.id);
	}

	getGrouppedTemplates(id){
		let token = localStorage.getItem("jwt");
        this.http.get("https://localhost:44396/api/templatefiles/index/"+id, {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: GrouppedTemplates[] )=> {
			this.groups=response;
        }, err => {
            console.log(err);
		});
	}

	deleteGroup(id){
		let token = localStorage.getItem("jwt");
        this.http.delete("https://localhost:44396/api/templatefiles/deletegroup/"+id, {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: GrouppedTemplates[] )=> {
			this.getGrouppedTemplates(this.user.id);
        }, err => {
            console.log(err);
		});
	}

}
