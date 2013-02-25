function[] = reduced_dimension_als(train_file_name, test_file_name, epochs, lrate, num_features)

%   file reading
train_file_text = dlmread(train_file_name, '\t');
test_file_text = dlmread(test_file_name, '\t');

%   Create sparse matrix
sparse_matrix = sparse(train_file_text(:,1), train_file_text(:,2), train_file_text(:,3));

test_users = test_file_text(:,1);
test_items = test_file_text(:,2);
test_ratings = test_file_text(:,3);
test_num_entries = length(test_file_text(:,1));

train_num_users = size(sparse_matrix,1);
train_num_items = size(sparse_matrix,2);
train_num_entries = length(train_file_text(:,1));

[user,item,rating] = find(sparse_matrix);
max_user = max(user);
max_item = max(item);

P = rand(max_user, num_features);
Q = rand(max_item, num_features);

%   train
for itr=1:epochs

    for u=1:train_num_users
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

    for i=1:train_num_items
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
    err_train = 0;
    err_test = 0;
    
    for e=1:(train_num_entries*0.1)
        err_train = err_train + power((rating(e,1) - dot(P(user(e,1),:),transpose(Q(item(e,1),:)))),2);       
    end
    
    for e=1:(test_num_entries*0.1)
        err_test = err_test + power((test_ratings(e,1) - dot(P(test_users(e,1),:),transpose(Q(test_items(e,1),:)))),2);
    end
          
    err_train = sqrt(err_train /(train_num_entries * 0.1));
    err_test = sqrt(err_test / (test_num_entries * 0.1));
    fprintf(1,'Iteration: %d, Train error: %f, Test error: %f\n', itr, err_train, err_test);
    train_err_array(itr) = err_train;
    test_err_array(itr) = err_test;
    
end

plot(train_err_array);
hold on
p = plot(test_err_array);
set(p,'Color','red');
title('RMSE error curve');
xlabel('Iteration');
ylabel('RMSE error');
hold off

