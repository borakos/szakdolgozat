import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

enum Type{
	word=0,
	excel,
}

interface TemplateFile {
    id: number;
    name: string;
    path: string;
    localName: string;
	type: string;
	ownerId: number;
	groupId: number;
	ownerName: string;
	groupName: string;
}

interface GrouppedTemplates {
    id: number;
    groupName: string;
    description: string;
    latestVersion: number;
	fileNumber: number;
	usedVersion: number;
}

interface User {
    id: number;
    userName: string;
    nativeName: string;
    email: string;
	role: string;
	password: string;
}

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

}
