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


@Component({
	selector: 'app-templates',
	templateUrl: './templates.component.html',
	styleUrls: ['./templates.component.css']
})
export class TemplatesComponent implements OnInit {

	public templates: TemplateFile[];

	constructor(private http:HttpClient) { }

	ngOnInit() {
		this.getAllUser();
	}

	getAllUser(){
		let token = localStorage.getItem("jwt");
        this.http.get("https://localhost:44396/api/templatefiles/index", {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: TemplateFile[] )=> {
			this.templates=response;
			console.log(response);
			for(let i=0;i<response.length;i++){
				this.templates[i].type=Type[response[i].type];
			}
        }, err => {
            console.log(err);
		});
	}

}
