function[] = reduced_dimension_als(file_name, epochs, lrate, num_features)

%   file reading
file_text = dlmread(file_name, '\t');

%   Create sparse matrix
sparse_matrix = sparse(file_text(:,1), file_text(:,2), file_text(:,3));

num_users = size(sparse_matrix,1);
num_items = size(sparse_matrix,2);
num_entries = length(file_text(:,1));

[user,item,rating] = find(sparse_matrix);
max_user = max(user);
max_item = max(item);

P = rand(max_user, num_features);
Q = rand(max_item, num_features);

% size(user)
% size(item)
% size(rating)

%   train
for itr=1:epochs

    for u=1:num_users
        nu = nnz(sparse_matrix(u,:));
        if (nu >= 1)
            [user,item,rating] = find(sparse_matrix(u,:));
            rating = transpose(rating);
            rating = vertcat(rating, zeros(num_features,1));

            ni = length(item);
            if (ni >= 1)
                qu = Q(item(1,1),:);
            end
            for t=2:ni
                qu = vertcat(qu, Q(item(1,t),:));
            end
            qu = vertcat(qu, diag(ones(1,num_features)*lrate));
            pu = lsqnonneg(qu, rating);
            P(u,:) = pu;
        end
    end

    for i=1:num_items
         nu = nnz(sparse_matrix(:,i));
         if (nu >= 1)
             [user,item,rating] = find(sparse_matrix(:,i));
             rating = vertcat(rating, zeros(num_features,1));

             ni = size(user,1);
             if (ni >= 1)
                 pi = P(user(1,1),:);
             end
             for t=2:ni
                 pi = vertcat(pi, P(user(t,1),:));
             end
             pi = vertcat(pi, diag(ones(1,num_features)*lrate));           
             qu = lsqnonneg(pi, rating);
             Q(i,:) = transpose(qu);
         end
    end
    
    [user,item,rating] = find(sparse_matrix);
    err = 0;
    
    for e=1:(num_entries*0.1)
        err = err + power((rating(e,1) - dot(P(user(e,1),:),transpose(Q(item(e,1),:)))),2);
    end
          
    err = sqrt(err/(num_entries*0.1));
    fprintf(1,'Iteration: %d, error: %f\n', itr, err);
    err_array(itr) = err;
    
end

plot(err_array);
title('RMSE error curve');
xlabel('Iteration');
ylabel('RMSE error');

