
let dataTable;

$(function () {
	loadDataTable();
});

function loadDataTable() {
	dataTable = $('#tblData').DataTable({
		"ajax": { url: '/admin/company/getall' },
		"columns": [
			{ data: 'name', "width": "15%", className: 'text-center' },
			{ data: 'streetAddress', "width": "20%", className: 'text-center' },
			{ data: 'phoneNumber', "width": "15%", className: 'text-center' },
			{ data: 'city', "width": "10%", className: 'text-center' },
			{ data: 'state', "width": "10%", className: 'text-center' },			
			{ data: 'postalCode', "width": "10%", className: 'text-center' },
			{
				data: 'id',
				width: "20%",
				className: 'text-center',
				render: function (data) {
					return `<div class="btn-group w-75" role="group">
						    <a href = "/admin/company/upsert?id=${data}" class="btn btn-success mx-2" >
							<i class="bi bi-pencil"></i> Edit </a >
							<a onClick= Delete('/admin/company/delete/${data}') class="btn btn-danger mx-2" >
							<i class="bi bi-trash3"></i> Delete </a >
							</div>`
				},
			},
		]
	});
}

function Delete(url) {
	Swal.fire({
		title: "Are you sure?",
		text: "You won't be able to revert this!",
		icon: "warning",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "Yes, delete it!"
	}).then((result) => {
		if (result.isConfirmed) {
			$.ajax({
				url: url,
				type: "DELETE",
				success: function (data) {
					if (data.success) {
						toastr.success(data.message);
						dataTable.ajax.reload();
					}
					else {
						toastr.error(data.message);
					}
				},
				error: function (xhr) {
					toastr.error("Something went wrong while deleting.");
				}
			});
		}
	});
}




