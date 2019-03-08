import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { User, GrouppedTemplates } from './../services/interfaces';
import { TemplateService } from './../services/templateservice';
import { Observable } from 'rxjs';

@Component({
	selector: 'app-templates',
	templateUrl: './templates.component.html',
	styleUrls: ['./templates.component.css']
})
export class TemplatesComponent implements OnInit {

	groups: Observable<GrouppedTemplates[]>;
	user: User;
	templateService= new TemplateService(this.http);

	constructor(private http:HttpClient) { }

	ngOnInit() {
		this.user=JSON.parse(localStorage.getItem('user'));
		this.getGrouppedTemplates(this.user.id);
	}

	getGrouppedTemplates(id){
		this.groups=this.templateService.getGrouppedTemplates(id);
        this.groups.subscribe((response)=> {}, err => {
            console.log(err);
		});
	}

	deleteGroup(id){
		let deletion = this.templateService.deleteGroup(id);
        deletion.subscribe((response)=> {
			this.getGrouppedTemplates(this.user.id);
        }, err => {
            console.log(err);
		});
	}

}
